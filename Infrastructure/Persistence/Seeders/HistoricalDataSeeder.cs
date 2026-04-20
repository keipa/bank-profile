using BankProfiles.Web.Infrastructure.Persistence.DbContext;
using BankProfiles.Web.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Web.Infrastructure.Persistence.Seeders;

public class HistoricalDataSeeder
{
    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<HistoricalDataSeeder> _logger;
    private readonly Random _random = new Random(42); // Fixed seed for reproducibility
    private const int DaysOfHistory = 90;

    public HistoricalDataSeeder(
        IDbContextFactory<BankDbContext> contextFactory,
        ILogger<HistoricalDataSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedHistoricalDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting historical data seeding");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        // Check if historical data already exists
        var hasRatingHistory = await context.RatingHistories.AnyAsync(cancellationToken);
        var hasViewHistory = await context.ViewHistory.AnyAsync(cancellationToken);

        if (hasRatingHistory || hasViewHistory)
        {
            _logger.LogInformation("Historical data already exists. Skipping seeding.");
            return;
        }

        var banks = await context.Banks.ToListAsync(cancellationToken);
        var criteriaIds = await context.RatingCriterias.Select(c => c.CriteriaId).ToListAsync(cancellationToken);
        var bankRatings = await context.BankRatings.ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Seeding historical data for {BankCount} banks over {DaysOfHistory} days",
            banks.Count,
            DaysOfHistory);

        var ratingHistories = new List<RatingHistory>();
        var viewHistories = new List<ViewHistory>();

        foreach (var bank in banks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pattern = GetBankPattern(bank.BankCode);
            var baseViewCount = bank.ViewCount;

            _logger.LogDebug("Generating historical data for {BankCode} with pattern {Pattern}", bank.BankCode, pattern);

            // Generate daily data for the last 90 days
            for (int daysAgo = DaysOfHistory - 1; daysAgo >= 0; daysAgo--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var recordDate = DateTime.UtcNow.Date.AddDays(-daysAgo);

                // Generate rating history for each criteria
                foreach (var criteriaId in criteriaIds)
                {
                    var currentRating = bankRatings.FirstOrDefault(r => r.BankId == bank.BankId && r.CriteriaId == criteriaId);
                    if (currentRating != null)
                    {
                        var historicalRating = GenerateHistoricalRating(
                            currentRating.RatingValue,
                            daysAgo,
                            pattern,
                            bank.BankCode,
                            criteriaId
                        );

                        ratingHistories.Add(new RatingHistory
                        {
                            BankId = bank.BankId,
                            CriteriaId = criteriaId,
                            OverallRating = historicalRating,
                            RecordedDate = recordDate
                        });
                    }
                }

                // Generate view history
                var historicalViewCount = GenerateHistoricalViewCount(
                    baseViewCount,
                    daysAgo,
                    pattern
                );

                viewHistories.Add(new ViewHistory
                {
                    BankId = bank.BankId,
                    ViewCount = historicalViewCount,
                    RecordedDate = recordDate
                });
            }
        }

        _logger.LogInformation("Generated {RatingCount} rating history records", ratingHistories.Count);
        _logger.LogInformation("Generated {ViewCount} view history records", viewHistories.Count);

        // Batch insert for performance
        await context.RatingHistories.AddRangeAsync(ratingHistories, cancellationToken);
        await context.ViewHistory.AddRangeAsync(viewHistories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Historical data seeding completed successfully");
    }

    private BankPattern GetBankPattern(string bankCode) =>
       bankCode switch
       {
          "bank-alpha" => BankPattern.TrendingUp,      // Growing steadily
          "bank-beta" => BankPattern.TrendingDown,     // Declining
          "bank-gamma" => BankPattern.Stable,          // Consistent
          "bank-delta" => BankPattern.Volatile,        // Fluctuating
          "bank-epsilon" => BankPattern.TrendingUp,    // Growing strongly
          _ => BankPattern.Stable
       };

    private decimal GenerateHistoricalRating(
        decimal currentRating,
        int daysAgo,
        BankPattern pattern,
        string bankCode,
        int criteriaId)
    {
        // Start from a base rating and trend towards current
        decimal baseRating = currentRating;
        decimal variance = 0m;

        switch (pattern)
        {
            case BankPattern.TrendingUp:
                // Ratings were lower in the past and improved
                variance = -(daysAgo / (decimal)DaysOfHistory) * 1.5m;
                break;

            case BankPattern.TrendingDown:
                // Ratings were higher in the past and declined
                variance = (daysAgo / (decimal)DaysOfHistory) * 1.2m;
                break;

            case BankPattern.Stable:
                // Minimal change with small random fluctuations
                variance = ((decimal)_random.NextDouble() - 0.5m) * 0.3m;
                break;

            case BankPattern.Volatile:
                // Significant random fluctuations
                var seed = daysAgo * criteriaId;
                var volatilityRandom = new Random(seed);
                variance = ((decimal)volatilityRandom.NextDouble() - 0.5m) * 2.0m;
                break;
        }

        // Add small daily noise
        var dailyNoise = ((decimal)_random.NextDouble() - 0.5m) * 0.2m;
        var rating = baseRating + variance + dailyNoise;

        // Clamp to valid range (6.0 - 9.5)
        rating = Math.Max(6.0m, Math.Min(9.5m, rating));

        // Round to 1 decimal place
        return Math.Round(rating, 1);
    }

    private int GenerateHistoricalViewCount(
        long currentViewCount,
        int daysAgo,
        BankPattern pattern)
    {
        // Calculate historical view count with growth trend
        double growthFactor = 1.0 - (daysAgo / (double)DaysOfHistory * 0.7);
        int baseViewCount = (int)(currentViewCount * growthFactor);

        // Apply pattern-specific adjustments
        double patternMultiplier = pattern switch
        {
            BankPattern.TrendingUp => 1.0 + (DaysOfHistory - daysAgo) / (double)DaysOfHistory * 0.3,
            BankPattern.TrendingDown => 1.0 - (DaysOfHistory - daysAgo) / (double)DaysOfHistory * 0.2,
            BankPattern.Volatile => 1.0 + (Math.Sin(daysAgo / 7.0) * 0.2),
            _ => 1.0
        };

        baseViewCount = (int)(baseViewCount * patternMultiplier);

        // Add daily variance (±20%)
        var variance = _random.Next(-20, 20) / 100.0;
        var viewCount = (int)(baseViewCount * (1 + variance));

        // Ensure minimum of 1 view per day
        return Math.Max(1, viewCount);
    }

    private enum BankPattern
    {
        TrendingUp,
        TrendingDown,
        Stable,
        Volatile
    }
}
