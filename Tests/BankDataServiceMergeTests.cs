using System.Text.Json;
using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using BankProfiles.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace BankProfiles.Tests;

public class BankDataServiceMergeTests
{
    private static readonly string[] EventBackedBankCodes = { "bank-event" };

    [Fact]
    public async Task GetAllBanksAsync_MergesEventAndJsonSources()
    {
        var tempDataDirectory = Path.Combine(Path.GetTempPath(), $"bank-data-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDataDirectory);

        var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 32 * 1024 * 1024 });

        try
        {
            var jsonBank = CreateProfile("bank-json", "JSON Bank", "United States");
            await File.WriteAllTextAsync(
                Path.Combine(tempDataDirectory, "bank-json.json"),
                JsonSerializer.Serialize(jsonBank));

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["BankDataSettings:DataDirectory"] = tempDataDirectory,
                    ["CacheSettings:AbsoluteExpirationMinutes"] = "60",
                    ["CacheSettings:SizeLimitMB"] = "32"
                })
                .Build();

            var cacheManager = new CacheManager(cache, configuration);
            var eventStore = new StubEventStoreService(EventBackedBankCodes);
            var projection = new StubProjectionService(CreateProfile("bank-event", "Event Bank", "United Kingdom"));
            var sut = new BankDataService(
                cacheManager,
                projection,
                eventStore,
                configuration,
                TestDbContextFactory.CreateLogger<BankDataService>());

            var banks = await sut.GetAllBanksAsync();

            Assert.Equal(2, banks.Count);
            Assert.Contains(banks, b => b.BankCode == "bank-event");
            Assert.Contains(banks, b => b.BankCode == "bank-json");
        }
        finally
        {
            cache.Dispose();
            Directory.Delete(tempDataDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task GetAllBanksAsync_SanitizesInvalidBrandingPaths()
    {
        var tempDataDirectory = Path.Combine(Path.GetTempPath(), $"bank-data-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDataDirectory);

        var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 32 * 1024 * 1024 });

        try
        {
            var jsonBank = CreateProfile("bank-json", "JSON Bank", "United States");
            jsonBank.Overview!.LogoUrl = "/images/banks/../json-logo.png";
            jsonBank.Overview.IconUrl = "/images/banks/json-icon.svg";

            await File.WriteAllTextAsync(
                Path.Combine(tempDataDirectory, "bank-json.json"),
                JsonSerializer.Serialize(jsonBank));

            var eventBank = CreateProfile("bank-event", "Event Bank", "United Kingdom");
            eventBank.Overview!.LogoUrl = "https://cdn.example.com/event-logo.png";
            eventBank.Overview.IconUrl = "/images/banks/event-icon.svg";

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["BankDataSettings:DataDirectory"] = tempDataDirectory,
                    ["CacheSettings:AbsoluteExpirationMinutes"] = "60",
                    ["CacheSettings:SizeLimitMB"] = "32"
                })
                .Build();

            var cacheManager = new CacheManager(cache, configuration);
            var eventStore = new StubEventStoreService(EventBackedBankCodes);
            var projection = new StubProjectionService(eventBank);
            var sut = new BankDataService(
                cacheManager,
                projection,
                eventStore,
                configuration,
                TestDbContextFactory.CreateLogger<BankDataService>());

            var banks = await sut.GetAllBanksAsync();

            var projectedBank = banks.Single(b => b.BankCode == "bank-event");
            Assert.Null(projectedBank.Overview?.LogoUrl);
            Assert.Equal("/images/banks/event-icon.svg", projectedBank.Overview?.IconUrl);

            var jsonBackedBank = banks.Single(b => b.BankCode == "bank-json");
            Assert.Null(jsonBackedBank.Overview?.LogoUrl);
            Assert.Equal("/images/banks/json-icon.svg", jsonBackedBank.Overview?.IconUrl);
        }
        finally
        {
            cache.Dispose();
            Directory.Delete(tempDataDirectory, recursive: true);
        }
    }

    private static BankProfile CreateProfile(string bankCode, string name, string headquartersCountry)
    {
        return new BankProfile
        {
            BankId = bankCode,
            Name = name,
            LegalName = $"{name} Ltd.",
            Status = "active",
            CountryOfOwnerResidence = headquartersCountry,
            HeadquartersCountry = headquartersCountry,
            Overview = new BankOverview
            {
                Type = "commercial bank",
                Segment = "Retail",
                Description = $"{name} profile"
            },
            Systems = new BankSystems
            {
                CardSystems = new List<string> { "visa", "mastercard" },
                SwiftAvailable = true,
                IbanSupported = true,
                SepaAvailable = true
            },
            Currencies = new BankCurrencies
            {
                Available = new List<string> { "USD" },
                BaseCurrency = "USD"
            },
            Fees = new BankFees
            {
                Commissions = new FeesCommissions(),
                AccountFees = new FeesAccount(),
                CardFees = new FeesCard(),
                TransferFees = new FeesTransfer()
            },
            Branches = new BankBranches { Count = 1 },
            Clients = new BankClients { Total = 1_000 },
            Ratings = new BankRatings { Overall = 4.0 },
            Compliance = new BankCompliance
            {
                SanctionsRisk = "low",
                AmlStatus = "good",
                KycStatus = "complete"
            },
            DigitalChannels = new DigitalChannels()
        };
    }

    private sealed class StubProjectionService : IEventProjectionService
    {
        private readonly BankProfile _profile;

        public StubProjectionService(BankProfile profile)
        {
            _profile = profile;
        }

        public Task<BankProfile?> ProjectBankProfileAsync(string bankCode)
        {
            return Task.FromResult<BankProfile?>(_profile.BankCode == bankCode ? _profile : null);
        }

        public BankProfile? ProjectFromEvents(List<MetricEvent> events)
        {
            return _profile;
        }
    }

    private sealed class StubEventStoreService : IEventStoreService
    {
        private readonly HashSet<string> _bankCodes;

        public StubEventStoreService(IEnumerable<string> bankCodes)
        {
            _bankCodes = new HashSet<string>(bankCodes, StringComparer.OrdinalIgnoreCase);
        }

        public Task<MetricEvent> AppendEventAsync(MetricEvent evt) => throw new NotSupportedException();
        public Task<List<MetricEvent>> AppendEventsAsync(List<MetricEvent> events) => throw new NotSupportedException();
        public Task<List<MetricEvent>> GetEventsForBankAsync(string bankCode) => throw new NotSupportedException();
        public Task<List<MetricEvent>> GetEventsByMetricAsync(string bankCode, string metricName) => throw new NotSupportedException();
        public Task<List<MetricEvent>> GetEventsInRangeAsync(string bankCode, DateTime from, DateTime to) => throw new NotSupportedException();

        public Task<List<string>> GetAllBankCodesAsync()
        {
            return Task.FromResult(_bankCodes.ToList());
        }

        public Task<long> GetLatestSequenceAsync(string bankCode) => throw new NotSupportedException();

        public Task<bool> HasEventsAsync(string bankCode)
        {
            return Task.FromResult(_bankCodes.Contains(bankCode));
        }
    }
}
