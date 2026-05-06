using System.Globalization;
using System.Text.Json;
using BankProfiles.Web.Application.Features.EventSourcing.Services;
using BankProfiles.Web.Application.Features.MetricCharts;
using BankProfiles.Web.Domain.BankProfiles;
using BankProfiles.Web.Domain.Common;
using BankProfiles.Web.Infrastructure.Persistence.DbContext;
using BankProfiles.Web.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Web.Infrastructure.Persistence.Seeders;

public class MetricEventHistoricalSeeder
{
    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<MetricEventHistoricalSeeder> _logger;
    private readonly string _dataDirectory;

    private const int DaysOfHistory = 90;
    private const int EventVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HashSet<string> WholeNumberMetricNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "overview.foundedYear",
        "metrics.openIssues",
        "metrics.resolvedEvents",
        "clients.total",
        "clients.retail",
        "clients.business",
        "clients.corporate",
        "clients.privateBanking",
        "branches.count",
        "branches.atmCount",
        "branches.partnerAtmNetwork",
        "fees.accountFees.dormancyAfterMonths",
        "metrics.avgRemediationDays",
        "digitalChannels.averageAccountOpeningMinutes",
        "support.averageResponseTimeMinutes"
    };

    private static readonly HashSet<string> ImmutableMetricNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "overview.foundedYear"
    };

    public MetricEventHistoricalSeeder(
        IDbContextFactory<BankDbContext> contextFactory,
        IConfiguration configuration,
        ILogger<MetricEventHistoricalSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _dataDirectory = configuration.GetValue<string>("BankDataSettings:DataDirectory")
            ?? "wwwroot/data/banks";
    }

    public async Task SeedMetricEventHistoryAsync(bool forceReseed = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting metric event history seeding. Force reseed: {ForceReseed}", forceReseed);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var banks = await context.Banks
            .AsNoTracking()
            .OrderBy(b => b.BankCode)
            .ToListAsync(cancellationToken);

        if (banks.Count == 0)
        {
            _logger.LogWarning("No banks found. Metric event history seeding skipped.");
            return;
        }

        var seedTimestamp = DateTime.UtcNow;
        var allEvents = new List<MetricEvent>();
        var generatedBankCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var skippedExistingBankCount = 0;
        var skippedMissingProfileBankCount = 0;

        foreach (var bank in banks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!forceReseed)
            {
                var bankHasEvents = await context.MetricEvents
                    .AnyAsync(e => e.BankCode == bank.BankCode, cancellationToken);
                if (bankHasEvents)
                {
                    skippedExistingBankCount++;
                    _logger.LogInformation(
                        "Metric events already exist for bank {BankCode}. Skipping this bank.",
                        bank.BankCode);
                    continue;
                }
            }

            if (!ValidationHelper.IsValidBankCode(bank.BankCode))
            {
                _logger.LogWarning("Invalid bank code in Banks table. Skipping metric seeding for {BankCode}", bank.BankCode);
                continue;
            }

            var profile = await LoadProfileAsync(bank.BankCode, cancellationToken);
            if (profile == null)
            {
                _logger.LogWarning("No valid profile found for bank {BankCode}. Skipping metric seeding for this bank.", bank.BankCode);
                skippedMissingProfileBankCount++;
                continue;
            }

            var bankEvents = BuildBankEvents(bank.BankCode, profile, seedTimestamp);
            if (bankEvents.Count == 0)
            {
                _logger.LogWarning("No metric events generated for bank {BankCode}.", bank.BankCode);
                continue;
            }

            allEvents.AddRange(bankEvents);
            generatedBankCodes.Add(bank.BankCode);
        }

        if (allEvents.Count == 0)
        {
            if (forceReseed)
            {
                throw new InvalidOperationException(
                    "Force reseed failed: no replacement metric events were generated.");
            }

            if (!forceReseed && skippedExistingBankCount == banks.Count)
            {
                _logger.LogInformation("All banks already have metric events. Nothing to seed.");
            }
            else
            {
                _logger.LogWarning("No metric events were generated for any bank.");
            }

            return;
        }

        if (forceReseed)
        {
            var existingEvents = await context.MetricEvents
                .Where(e => generatedBankCodes.Contains(e.BankCode))
                .ToListAsync(cancellationToken);
            if (existingEvents.Count > 0)
            {
                context.MetricEvents.RemoveRange(existingEvents);
            }

            var existingSnapshots = await context.BankSnapshots
                .Where(s => generatedBankCodes.Contains(s.BankCode))
                .ToListAsync(cancellationToken);
            if (existingSnapshots.Count > 0)
            {
                context.BankSnapshots.RemoveRange(existingSnapshots);
            }

            _logger.LogInformation(
                "Existing MetricEvents/BankSnapshots scheduled for replacement for {BankCount} banks.",
                generatedBankCodes.Count);
        }

        await context.MetricEvents.AddRangeAsync(allEvents, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Metric event history seeding completed. Generated {EventCount} events for {GeneratedBankCount} banks. Missing profile banks skipped: {MissingProfileSkipped}.",
            allEvents.Count,
            generatedBankCodes.Count,
            skippedMissingProfileBankCount);
    }

    private async Task<BankProfile?> LoadProfileAsync(string bankCode, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(_dataDirectory, $"{bankCode}.json");
        if (!File.Exists(filePath))
        {
            return null;
        }

        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<BankProfile>(stream, JsonOptions, cancellationToken);
    }

    private List<MetricEvent> BuildBankEvents(string bankCode, BankProfile profile, DateTime seedTimestamp)
    {
        var country = string.IsNullOrWhiteSpace(profile.HeadquartersCountry)
            ? "unknown"
            : profile.HeadquartersCountry;
        var pattern = GetBankPattern(bankCode);
        var baselineDate = seedTimestamp.Date.AddHours(23).AddMinutes(45);

        var baselineEvents = EventMigrationService.FlattenProfileToEvents(profile, "Seed baseline snapshot");
        foreach (var baselineEvent in baselineEvents)
        {
            baselineEvent.BankCode = bankCode;
            baselineEvent.Country = country;
            baselineEvent.Comment = "Seed baseline snapshot";
            baselineEvent.EventVersion = EventVersion;
            baselineEvent.CreatedDate = baselineDate;
        }

        var historicalEvents = new List<MetricEvent>();

        foreach (var baselineMetric in baselineEvents)
        {
            if (!MetricChartMappings.EventMetricNames.Contains(baselineMetric.MetricName))
                continue;

            if (!TryParseNumericMetricValue(baselineMetric.MetricValue, out var currentValue))
                continue;

            for (var daysAgo = DaysOfHistory; daysAgo >= 1; daysAgo--)
            {
                var historicalTimestamp = seedTimestamp.Date
                    .AddDays(-daysAgo)
                    .AddHours(GetHourOffset(baselineMetric.MetricName))
                    .AddMinutes(GetMinuteOffset(baselineMetric.MetricName));

                var historicalValue = GenerateHistoricalValue(
                    currentValue,
                    daysAgo,
                    pattern,
                    bankCode,
                    baselineMetric.MetricName,
                    baselineMetric.MetricType);

                historicalEvents.Add(new MetricEvent
                {
                    BankCode = bankCode,
                    Country = country,
                    MetricName = baselineMetric.MetricName,
                    MetricType = baselineMetric.MetricType,
                    MetricValue = SerializeMetricValue(baselineMetric.MetricName, historicalValue),
                    Comment = "Historical metric seed snapshot",
                    CreatedDate = historicalTimestamp,
                    EventVersion = EventVersion
                });
            }
        }

        var orderedEvents = historicalEvents
            .Concat(baselineEvents)
            .OrderBy(e => e.CreatedDate)
            .ThenBy(e => e.MetricName, StringComparer.Ordinal)
            .ToList();

        for (var i = 0; i < orderedEvents.Count; i++)
        {
            orderedEvents[i].EventSequence = i + 1;
        }

        return orderedEvents;
    }

    private static bool TryParseNumericMetricValue(string rawValue, out double value)
    {
        var cleaned = rawValue.Trim().Trim('"');
        return double.TryParse(
            cleaned,
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static string SerializeMetricValue(string metricName, double value)
    {
        if (WholeNumberMetricNames.Contains(metricName))
        {
            var whole = Math.Max(0L, (long)Math.Round(value, MidpointRounding.AwayFromZero));
            return JsonSerializer.Serialize(whole);
        }

        var rounded = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        return JsonSerializer.Serialize(rounded);
    }

    private static double GenerateHistoricalValue(
        double currentValue,
        int daysAgo,
        BankPattern pattern,
        string bankCode,
        string metricName,
        string metricType)
    {
        if (ImmutableMetricNames.Contains(metricName))
        {
            return currentValue;
        }

        var scale = Math.Max(Math.Abs(currentValue), 1.0);
        var progress = daysAgo / (double)DaysOfHistory;
        var seasonal = Math.Sin((DaysOfHistory - daysAgo + ((StableHash(metricName) & 0x7FFFFFFF) % 11)) / 7.0);

        var trend = pattern switch
        {
            BankPattern.TrendingUp => -progress * scale * 0.18,
            BankPattern.TrendingDown => progress * scale * 0.15,
            BankPattern.Volatile => seasonal * scale * 0.12,
            _ => seasonal * scale * 0.04
        };

        var noise = DeterministicNoise(bankCode, metricName, daysAgo, scale * 0.03);
        var candidate = currentValue + trend + noise;

        if (string.Equals(metricType, "Percentage", StringComparison.OrdinalIgnoreCase))
        {
            candidate = Math.Clamp(candidate, 0, 100);
        }
        else if (string.Equals(metricName, "ratings.overall", StringComparison.OrdinalIgnoreCase))
        {
            candidate = Math.Clamp(candidate, 0, 5);
        }
        else
        {
            candidate = Math.Max(0, candidate);
        }

        return candidate;
    }

    private static double DeterministicNoise(string bankCode, string metricName, int daysAgo, double amplitude)
    {
        var normalized = (StableHash($"{bankCode}|{metricName}|{daysAgo}") & 0x7FFFFFFF) / (double)int.MaxValue;
        return (normalized - 0.5) * 2 * amplitude;
    }

    private static int GetHourOffset(string metricName) => (StableHash($"{metricName}|h") & 0x7FFFFFFF) % 24;

    private static int GetMinuteOffset(string metricName) => (StableHash($"{metricName}|m") & 0x7FFFFFFF) % 60;

    private static int StableHash(string value)
    {
        unchecked
        {
            var hash = (int)2166136261;
            foreach (var c in value)
            {
                hash ^= c;
                hash *= 16777619;
            }

            return hash;
        }
    }

    private static BankPattern GetBankPattern(string bankCode) =>
        bankCode switch
        {
            "bank-alpha" => BankPattern.TrendingUp,
            "bank-beta" => BankPattern.TrendingDown,
            "bank-gamma" => BankPattern.Stable,
            "bank-delta" => BankPattern.Volatile,
            "bank-epsilon" => BankPattern.TrendingUp,
            _ => BankPattern.Stable
        };

    private enum BankPattern
    {
        TrendingUp,
        TrendingDown,
        Stable,
        Volatile
    }
}
