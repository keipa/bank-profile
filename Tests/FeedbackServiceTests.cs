using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using BankProfiles.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace BankProfiles.Tests;

public class FeedbackServiceTests
{
    private readonly IDbContextFactory<BankDbContext> _factory;
    private readonly FeedbackService _sut;
    private readonly TestCacheManager _cacheManager;
    private readonly StubEventMigrationService _eventMigrationService;

    public FeedbackServiceTests()
    {
        _factory = TestDbContextFactory.Create(Guid.NewGuid().ToString());
        _cacheManager = new TestCacheManager();
        _eventMigrationService = new StubEventMigrationService();

        var eventStore = new EventStoreService(_factory, TestDbContextFactory.CreateLogger<EventStoreService>());
        var bankDataService = new StubBankDataService(CreateBankProfile("bank-alpha"), CreateBankProfile("bank-beta"));

        _sut = new FeedbackService(
            _factory,
            eventStore,
            _eventMigrationService,
            bankDataService,
            _cacheManager,
            new StubWebHostEnvironment(Environments.Development),
            TestDbContextFactory.CreateLogger<FeedbackService>());
    }

    [Fact]
    public async Task SubmitFeedbackAsync_PersistsFeedbackAndTracksSubmission()
    {
        await EnsureBankAsync("bank-alpha");

        const string ip = "127.0.0.1";
        var feedback = CreateValidFeedback(ip, "bank-alpha");
        feedback.Explanation = "Please update this metric value <script>alert('x')</script> based on the latest report.";

        var success = await _sut.SubmitFeedbackAsync(feedback);

        Assert.True(success);

        await using var context = await _factory.CreateDbContextAsync();
        var storedFeedback = await context.MetricFeedbacks.SingleAsync();

        Assert.Equal(ip, storedFeedback.SubmitterIP);
        Assert.Equal("bank-alpha", storedFeedback.BankCode);
        Assert.NotNull(storedFeedback.BankId);
        Assert.Equal("fees.commissions.incomingDomesticPercent", storedFeedback.MetricPath);
        Assert.Equal(MetricFeedbackStatuses.Pending, storedFeedback.Status);
        Assert.DoesNotContain("<script", storedFeedback.Explanation, StringComparison.OrdinalIgnoreCase);

        var submissionCount = await context.FeedbackSubmissions.CountAsync();
        Assert.Equal(1, submissionCount);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_ReturnsFalse_WhenRequiredFieldsMissing()
    {
        var invalid = CreateValidFeedback("127.0.0.2", "bank-alpha");
        invalid.MetricName = "";

        var success = await _sut.SubmitFeedbackAsync(invalid);

        Assert.False(success);

        await using var context = await _factory.CreateDbContextAsync();
        Assert.Empty(context.MetricFeedbacks);
        Assert.Empty(context.FeedbackSubmissions);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_ReturnsFalse_WhenBankCodeDoesNotExist()
    {
        var feedback = CreateValidFeedback("127.0.0.9", "bank-missing");

        var success = await _sut.SubmitFeedbackAsync(feedback);

        Assert.False(success);

        await using var context = await _factory.CreateDbContextAsync();
        Assert.Empty(context.MetricFeedbacks);
        Assert.Empty(context.FeedbackSubmissions);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_EnforcesDailyRateLimit()
    {
        await EnsureBankAsync("bank-alpha");

        const string ip = "127.0.0.3";

        for (var i = 0; i < 10; i++)
        {
            var result = await _sut.SubmitFeedbackAsync(CreateValidFeedback(ip, "bank-alpha"));
            Assert.True(result);
        }

        var blocked = await _sut.SubmitFeedbackAsync(CreateValidFeedback(ip, "bank-alpha"));
        Assert.False(blocked);

        var remaining = await _sut.GetRemainingSubmissionsAsync(ip);
        Assert.Equal(0, remaining);
    }

    [Fact]
    public async Task ApproveFeedbackAsync_AppendsEventAndMarksFeedbackApproved()
    {
        await EnsureBankAsync("bank-alpha");
        await _sut.SubmitFeedbackAsync(CreateValidFeedback("127.0.0.4", "bank-alpha"));

        _cacheManager.Set("bank_bank-alpha", new object());
        _cacheManager.Set("all_banks", new object());

        await using (var readContext = await _factory.CreateDbContextAsync())
        {
            var feedback = await readContext.MetricFeedbacks.SingleAsync();
            var result = await _sut.ApproveFeedbackAsync(feedback.FeedbackId, "admin-user", "Looks correct");

            Assert.True(result.Success);
            Assert.True(result.AppliedEventId.HasValue);
        }

        await using var context = await _factory.CreateDbContextAsync();
        var storedFeedback = await context.MetricFeedbacks.SingleAsync();
        var events = await context.MetricEvents.Where(e => e.BankCode == "bank-alpha").ToListAsync();

        Assert.Equal(MetricFeedbackStatuses.Approved, storedFeedback.Status);
        Assert.Equal("admin-user", storedFeedback.ReviewedBy);
        Assert.NotNull(storedFeedback.ReviewedDate);
        Assert.Equal("Looks correct", storedFeedback.ReviewNotes);
        Assert.NotNull(storedFeedback.AppliedEventId);

        Assert.Single(events);
        Assert.Equal("fees.commissions.incomingDomesticPercent", events[0].MetricName);
        Assert.Equal("1.9", events[0].MetricValue);
        Assert.Equal("Percentage", events[0].MetricType);

        Assert.Contains("bank_bank-alpha", _cacheManager.RemovedKeys);
        Assert.Contains("all_banks", _cacheManager.RemovedKeys);
        Assert.Equal(1, _eventMigrationService.SingleBankCallCount);
    }

    [Fact]
    public async Task ApproveFeedbackAsync_ReturnsError_WhenSuggestedValueCannotBeParsed()
    {
        await EnsureBankAsync("bank-alpha");
        var feedback = CreateValidFeedback("127.0.0.5", "bank-alpha");
        feedback.MetricCategory = "systems";
        feedback.MetricName = "swiftAvailable";
        feedback.SuggestedValue = "not-a-bool";
        await _sut.SubmitFeedbackAsync(feedback);

        await using var readContext = await _factory.CreateDbContextAsync();
        var stored = await readContext.MetricFeedbacks.SingleAsync();
        var result = await _sut.ApproveFeedbackAsync(stored.FeedbackId, "admin-user", null);

        Assert.False(result.Success);
        Assert.Equal(FeedbackModerationError.InvalidSuggestedValue, result.Error);
    }

    [Fact]
    public async Task ApproveFeedbackAsync_TruncatesEventCommentToConfiguredLength()
    {
        await EnsureBankAsync("bank-alpha");
        var feedback = CreateValidFeedback("127.0.0.10", "bank-alpha");
        feedback.Explanation = new string('x', 1500);
        await _sut.SubmitFeedbackAsync(feedback);

        await using var readContext = await _factory.CreateDbContextAsync();
        var stored = await readContext.MetricFeedbacks.SingleAsync();
        var result = await _sut.ApproveFeedbackAsync(stored.FeedbackId, "admin-user", null);

        Assert.True(result.Success);

        await using var verifyContext = await _factory.CreateDbContextAsync();
        var metricEvent = await verifyContext.MetricEvents.SingleAsync();
        Assert.NotNull(metricEvent.Comment);
        Assert.True(metricEvent.Comment!.Length <= 1000);
    }

    [Fact]
    public async Task RejectFeedbackAsync_MarksFeedbackRejectedWithoutEvent()
    {
        await EnsureBankAsync("bank-beta");
        await _sut.SubmitFeedbackAsync(CreateValidFeedback("127.0.0.6", "bank-beta"));

        await using (var readContext = await _factory.CreateDbContextAsync())
        {
            var feedback = await readContext.MetricFeedbacks.SingleAsync();
            var result = await _sut.RejectFeedbackAsync(feedback.FeedbackId, "moderator", "Outdated source");
            Assert.True(result.Success);
        }

        await using var context = await _factory.CreateDbContextAsync();
        var storedFeedback = await context.MetricFeedbacks.SingleAsync();
        var events = await context.MetricEvents.ToListAsync();

        Assert.Equal(MetricFeedbackStatuses.Rejected, storedFeedback.Status);
        Assert.Equal("moderator", storedFeedback.ReviewedBy);
        Assert.Equal("Outdated source", storedFeedback.ReviewNotes);
        Assert.Empty(events);
    }

    [Fact]
    public async Task ApproveFeedbackAsync_IsDisabledOutsideDevelopment()
    {
        await EnsureBankAsync("bank-alpha");
        await _sut.SubmitFeedbackAsync(CreateValidFeedback("127.0.0.7", "bank-alpha"));

        var eventStore = new EventStoreService(_factory, TestDbContextFactory.CreateLogger<EventStoreService>());
        var prodService = new FeedbackService(
            _factory,
            eventStore,
            _eventMigrationService,
            new StubBankDataService(CreateBankProfile("bank-alpha")),
            _cacheManager,
            new StubWebHostEnvironment(Environments.Production),
            TestDbContextFactory.CreateLogger<FeedbackService>());

        await using var context = await _factory.CreateDbContextAsync();
        var feedback = await context.MetricFeedbacks.SingleAsync();
        var result = await prodService.ApproveFeedbackAsync(feedback.FeedbackId, "admin", "review");

        Assert.False(result.Success);
        Assert.Equal(FeedbackModerationError.ModerationDisabled, result.Error);
    }

    private async Task EnsureBankAsync(string bankCode)
    {
        await using var context = await _factory.CreateDbContextAsync();
        if (await context.Banks.AnyAsync(b => b.BankCode == bankCode))
        {
            return;
        }

        context.Banks.Add(new Bank
        {
            BankCode = bankCode,
            ViewCount = 0,
            CreatedDate = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    private static MetricFeedback CreateValidFeedback(string ipAddress, string bankCode)
    {
        return new MetricFeedback
        {
            BankCode = bankCode,
            MetricCategory = "fees",
            MetricName = "incomingDomesticPercent",
            CurrentValue = "2.5",
            SuggestedValue = "1.9",
            Explanation = "The official pricing page was updated and now shows a lower incoming domestic transfer fee.",
            SubmitterIP = ipAddress,
            SubmittedDate = DateTime.UtcNow
        };
    }

    private static BankProfile CreateBankProfile(string bankCode)
    {
        return new BankProfile
        {
            BankId = bankCode,
            Name = $"Name {bankCode}",
            LegalName = $"Legal {bankCode}",
            Status = "active",
            CountryOfOwnerResidence = "United States",
            HeadquartersCountry = "United States",
            Systems = new BankSystems
            {
                CardSystems = new List<string>(),
                SwiftAvailable = true,
                IbanSupported = true,
                SepaAvailable = true
            },
            Currencies = new BankCurrencies
            {
                Available = new List<string>()
            },
            Fees = new BankFees
            {
                Commissions = new FeesCommissions(),
                AccountFees = new FeesAccount(),
                CardFees = new FeesCard(),
                TransferFees = new FeesTransfer()
            },
            Branches = new BankBranches { Count = 1 },
            Clients = new BankClients { Total = 1 },
            Ratings = new BankRatings { Overall = 1 },
            Compliance = new BankCompliance
            {
                SanctionsRisk = "low",
                AmlStatus = "good",
                KycStatus = "complete"
            },
            DigitalChannels = new DigitalChannels()
        };
    }

    private sealed class StubEventMigrationService : IEventMigrationService
    {
        public int SingleBankCallCount { get; private set; }

        public Task<MigrationResult> MigrateFromJsonAsync(bool dryRun = false)
        {
            return Task.FromResult(new MigrationResult { DryRun = dryRun });
        }

        public Task<MigrationResult> MigrateSingleBankAsync(string bankCode, bool dryRun = false)
        {
            SingleBankCallCount++;
            return Task.FromResult(new MigrationResult
            {
                DryRun = dryRun,
                BanksProcessed = 1,
                EventsCreated = 0
            });
        }
    }

    private sealed class StubBankDataService : IBankDataService
    {
        private readonly Dictionary<string, BankProfile> _banks;

        public StubBankDataService(params BankProfile[] banks)
        {
            _banks = banks.ToDictionary(b => b.BankCode, StringComparer.OrdinalIgnoreCase);
        }

        public Task<BankProfile?> GetBankByCodeAsync(string bankCode)
        {
            _banks.TryGetValue(bankCode, out var profile);
            return Task.FromResult(profile);
        }

        public Task<List<BankProfile>> GetAllBanksAsync()
        {
            return Task.FromResult(_banks.Values.ToList());
        }

        public Task RefreshCacheAsync()
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestCacheManager : ICacheManager
    {
        private readonly Dictionary<string, object> _items = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> RemovedKeys { get; } = new(StringComparer.OrdinalIgnoreCase);

        public T? Get<T>(string key)
        {
            if (_items.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            if (value == null)
            {
                return;
            }

            _items[key] = value;
        }

        public void Remove(string key)
        {
            _items.Remove(key);
            RemovedKeys.Add(key);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public CacheStatistics GetStatistics()
        {
            return new CacheStatistics
            {
                ItemCount = _items.Count
            };
        }
    }

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public StubWebHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
            ApplicationName = "BankProfiles.Tests";
            WebRootPath = string.Empty;
            ContentRootPath = string.Empty;
            WebRootFileProvider = new NullFileProvider();
            ContentRootFileProvider = new NullFileProvider();
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
