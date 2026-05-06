using BankProfiles.Tests.Infrastructure.Persistence;
using BankProfiles.Web.Infrastructure.Persistence.DbContext;
using BankProfiles.Web.Infrastructure.Persistence.Entities;
using BankProfiles.Web.Infrastructure.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BankProfiles.Tests.Features.Seeding;

public class MetricEventHistoricalSeederTests
{
    [Fact]
    public async Task SeedMetricEventHistoryAsync_GeneratesTimestampedMetricHistoryAndBaselineEvents()
    {
        var factory = TestDbContextFactory.Create(Guid.NewGuid().ToString("N"));
        var tempDataDirectory = CreateTempDataDirectoryWithBankJson("bank-alpha");

        try
        {
            await using (var setupContext = await factory.CreateDbContextAsync())
            {
                setupContext.Database.EnsureCreated();
                setupContext.Banks.Add(new Bank
                {
                    BankCode = "bank-alpha",
                    ViewCount = 100,
                    CreatedDate = DateTime.UtcNow
                });
                await setupContext.SaveChangesAsync();
            }

            var configuration = BuildConfiguration(tempDataDirectory);
            var sut = new MetricEventHistoricalSeeder(
                factory,
                configuration,
                TestDbContextFactory.CreateLogger<MetricEventHistoricalSeeder>());

            await sut.SeedMetricEventHistoryAsync();

            await using var verifyContext = await factory.CreateDbContextAsync();
            var events = await verifyContext.MetricEvents
                .Where(e => e.BankCode == "bank-alpha")
                .OrderBy(e => e.EventSequence)
                .ToListAsync();

            Assert.NotEmpty(events);

            var expectedSequence = Enumerable.Range(1, events.Count).Select(i => (long)i);
            Assert.Equal(expectedSequence, events.Select(e => e.EventSequence));

            var ratingHistory = events.Where(e => e.MetricName == "ratings.overall").ToList();
            Assert.True(ratingHistory.Count > 1);
            Assert.True(ratingHistory.Select(e => e.CreatedDate).Distinct().Count() > 1);

            var nameEvents = events.Where(e => e.MetricName == "name").ToList();
            Assert.Single(nameEvents);
        }
        finally
        {
            Directory.Delete(tempDataDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SeedMetricEventHistoryAsync_ForceReseed_ReplacesExistingMetricEvents()
    {
        var factory = TestDbContextFactory.Create(Guid.NewGuid().ToString("N"));
        var tempDataDirectory = CreateTempDataDirectoryWithBankJson("bank-alpha");

        try
        {
            await using (var setupContext = await factory.CreateDbContextAsync())
            {
                setupContext.Database.EnsureCreated();
                setupContext.Banks.Add(new Bank
                {
                    BankCode = "bank-alpha",
                    ViewCount = 100,
                    CreatedDate = DateTime.UtcNow
                });
                await setupContext.SaveChangesAsync();
            }

            var configuration = BuildConfiguration(tempDataDirectory);
            var sut = new MetricEventHistoricalSeeder(
                factory,
                configuration,
                TestDbContextFactory.CreateLogger<MetricEventHistoricalSeeder>());

            await sut.SeedMetricEventHistoryAsync();

            var initialCount = await CountEventsAsync(factory);
            Assert.True(initialCount > 0);

            await sut.SeedMetricEventHistoryAsync();
            var secondCount = await CountEventsAsync(factory);
            Assert.Equal(initialCount, secondCount);

            await sut.SeedMetricEventHistoryAsync(forceReseed: true);
            var reseededCount = await CountEventsAsync(factory);
            Assert.Equal(initialCount, reseededCount);

            await using var verifyContext = await factory.CreateDbContextAsync();
            var minSequence = await verifyContext.MetricEvents
                .Where(e => e.BankCode == "bank-alpha")
                .MinAsync(e => e.EventSequence);
            Assert.Equal(1, minSequence);
        }
        finally
        {
            Directory.Delete(tempDataDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SeedMetricEventHistoryAsync_ForceReseed_LeavesBanksWithoutProfilesUntouched()
    {
        var factory = TestDbContextFactory.Create(Guid.NewGuid().ToString("N"));
        var tempDataDirectory = CreateTempDataDirectoryWithBankJson("bank-alpha");

        try
        {
            await using (var setupContext = await factory.CreateDbContextAsync())
            {
                setupContext.Database.EnsureCreated();
                setupContext.Banks.AddRange(
                    new Bank
                    {
                        BankCode = "bank-alpha",
                        ViewCount = 100,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Bank
                    {
                        BankCode = "bank-prior",
                        ViewCount = 50,
                        CreatedDate = DateTime.UtcNow
                    });

                setupContext.MetricEvents.AddRange(
                    new MetricEvent
                    {
                        BankCode = "bank-alpha",
                        Country = "United Kingdom",
                        MetricName = "ratings.overall",
                        MetricValue = "1.5",
                        MetricType = "Numeric",
                        Comment = "legacy-alpha",
                        CreatedDate = DateTime.UtcNow.AddDays(-120),
                        EventVersion = 1,
                        EventSequence = 1
                    },
                    new MetricEvent
                    {
                        BankCode = "bank-prior",
                        Country = "United States",
                        MetricName = "ratings.overall",
                        MetricValue = "2.5",
                        MetricType = "Numeric",
                        Comment = "legacy-prior",
                        CreatedDate = DateTime.UtcNow.AddDays(-120),
                        EventVersion = 1,
                        EventSequence = 1
                    });

                await setupContext.SaveChangesAsync();
            }

            var configuration = BuildConfiguration(tempDataDirectory);
            var sut = new MetricEventHistoricalSeeder(
                factory,
                configuration,
                TestDbContextFactory.CreateLogger<MetricEventHistoricalSeeder>());

            await sut.SeedMetricEventHistoryAsync(forceReseed: true);

            await using var verifyContext = await factory.CreateDbContextAsync();

            var alphaEvents = await verifyContext.MetricEvents
                .Where(e => e.BankCode == "bank-alpha")
                .OrderBy(e => e.EventSequence)
                .ToListAsync();
            Assert.True(alphaEvents.Count > 1);
            Assert.DoesNotContain(alphaEvents, e => e.Comment == "legacy-alpha");
            Assert.Equal(1, alphaEvents.First().EventSequence);

            var priorEvents = await verifyContext.MetricEvents
                .Where(e => e.BankCode == "bank-prior")
                .OrderBy(e => e.EventSequence)
                .ToListAsync();
            Assert.Single(priorEvents);
            Assert.Equal("legacy-prior", priorEvents[0].Comment);
            Assert.Equal("2.5", priorEvents[0].MetricValue);
        }
        finally
        {
            Directory.Delete(tempDataDirectory, recursive: true);
        }
    }

    private static IConfiguration BuildConfiguration(string dataDirectory) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BankDataSettings:DataDirectory"] = dataDirectory
            })
            .Build();

    private static async Task<int> CountEventsAsync(IDbContextFactory<BankDbContext> factory)
    {
        await using var context = await factory.CreateDbContextAsync();
        return await context.MetricEvents.CountAsync(e => e.BankCode == "bank-alpha");
    }

    private static string CreateTempDataDirectoryWithBankJson(string bankCode)
    {
        var tempDataDirectory = Path.Combine(Path.GetTempPath(), $"metric-seed-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDataDirectory);

        var sourcePath = FindProjectFile(Path.Combine("wwwroot", "data", "banks", $"{bankCode}.json"));
        File.Copy(sourcePath, Path.Combine(tempDataDirectory, $"{bankCode}.json"));

        return tempDataDirectory;
    }

    private static string FindProjectFile(string relativePath)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        for (var i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(currentDirectory, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            var parent = Directory.GetParent(currentDirectory);
            if (parent == null)
            {
                break;
            }

            currentDirectory = parent.FullName;
        }

        throw new FileNotFoundException($"Could not locate test fixture file '{relativePath}'.");
    }
}
