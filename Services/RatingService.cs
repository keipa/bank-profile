using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Web.Services;

public interface IRatingService
{
    Task<List<RatingData>> GetBankRatingsAsync(string bankCode);
    Task<List<RatingHistoryPoint>> GetRatingHistoryAsync(string bankCode, int criteriaId, int days = 30);
    Task<decimal> GetOverallRatingAsync(string bankCode);
    Task<List<BankRatingsSummary>> GetAllBankRatingsAsync();
    Task AddRatingHistorySnapshotAsync();
}

public class RatingService : IRatingService
{
    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<RatingService> _logger;
    private readonly IBankDataService _bankDataService;

    public RatingService(IDbContextFactory<BankDbContext> contextFactory, ILogger<RatingService> logger, IBankDataService bankDataService)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _bankDataService = bankDataService;
    }

    public async Task<List<RatingData>> GetBankRatingsAsync(string bankCode)
    {
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return new List<RatingData>();
        }

        using var context = await _contextFactory.CreateDbContextAsync();
        
        var bank = await context.Banks
            .Include(b => b.BankRatings)
            .ThenInclude(br => br.Criteria)
            .FirstOrDefaultAsync(b => b.BankCode == bankCode);

        if (bank == null)
        {
            return new List<RatingData>();
        }

        var ratings = bank.BankRatings
            .GroupBy(br => br.Criteria)
            .Select(g => new RatingData
            {
                CriteriaName = g.Key.Name,
                CurrentRating = g.OrderByDescending(br => br.RatingDate).First().RatingValue
            })
            .ToList();

        return ratings;
    }

    public async Task<List<RatingHistoryPoint>> GetRatingHistoryAsync(string bankCode, int criteriaId, int days = 30)
    {
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return new List<RatingHistoryPoint>();
        }

        using var context = await _contextFactory.CreateDbContextAsync();
        
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var bank = await context.Banks
            .FirstOrDefaultAsync(b => b.BankCode == bankCode);

        if (bank == null)
        {
            return new List<RatingHistoryPoint>();
        }

        var history = await context.RatingHistories
            .Where(rh => rh.BankId == bank.BankId 
                && rh.CriteriaId == criteriaId 
                && rh.RecordedDate >= cutoffDate)
            .OrderBy(rh => rh.RecordedDate)
            .Select(rh => new RatingHistoryPoint
            {
                Date = rh.RecordedDate,
                Value = rh.OverallRating
            })
            .ToListAsync();

        return history;
    }

    public async Task<decimal> GetOverallRatingAsync(string bankCode)
    {
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return 0;
        }

        var ratings = await GetBankRatingsAsync(bankCode);
        
        if (!ratings.Any())
        {
            return 0;
        }

        return Math.Round(ratings.Average(r => r.CurrentRating), 2);
    }

    public async Task<List<BankRatingsSummary>> GetAllBankRatingsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var banks = await context.Banks
            .Include(b => b.BankRatings)
            .ThenInclude(br => br.Criteria)
            .ToListAsync();

        var summaries = new List<BankRatingsSummary>();

        foreach (var bank in banks)
        {
            // Load the full bank profile to get country code and name
            var bankProfile = await _bankDataService.GetBankByCodeAsync(bank.BankCode);
            
            var criteriaRatings = bank.BankRatings
                .GroupBy(br => br.Criteria)
                .Select(g => new RatingData
                {
                    CriteriaName = g.Key.Name,
                    CurrentRating = g.OrderByDescending(br => br.RatingDate).First().RatingValue
                })
                .ToList();

            var overallRating = criteriaRatings.Any() 
                ? Math.Round(criteriaRatings.Average(cr => cr.CurrentRating), 2) 
                : 0;

            summaries.Add(new BankRatingsSummary
            {
                BankCode = bank.BankCode,
                CountryCode = bankProfile?.CountryCode,
                BankName = bankProfile?.BankName ?? bank.BankCode,
                OverallRating = overallRating,
                CriteriaRatings = criteriaRatings
            });
        }

        return summaries;
    }

    public async Task AddRatingHistorySnapshotAsync()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var banks = await context.Banks
                .Include(b => b.BankRatings)
                .ToListAsync();

            foreach (var bank in banks)
            {
                var criteriaGroups = bank.BankRatings.GroupBy(br => br.CriteriaId);

                foreach (var group in criteriaGroups)
                {
                    var latestRating = group.OrderByDescending(br => br.RatingDate).First();
                    var overallRating = latestRating.RatingValue;

                    context.RatingHistories.Add(new RatingHistory
                    {
                        BankId = bank.BankId,
                        CriteriaId = group.Key,
                        OverallRating = overallRating,
                        RecordedDate = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Rating history snapshot created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating rating history snapshot");
            throw;
        }
    }
}
