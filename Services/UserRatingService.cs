using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Web.Services;

public class UserRatingService : IUserRatingService
{
    private static readonly string[] RequiredCriteriaNames =
    {
        "Service",
        "Fees",
        "Convenience",
        "Digital Services",
        "Customer Support"
    };

    private const int MaxSubmissionsPerDay = 10;
    private const int RateLimitWindowHours = 24;
    private const int MaxCommentLength = 1000;

    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<UserRatingService> _logger;
    private readonly decimal _minRating;
    private readonly decimal _maxRating;

    public UserRatingService(
        IDbContextFactory<BankDbContext> contextFactory,
        ILogger<UserRatingService> logger,
        IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _minRating = configuration.GetValue<decimal>("RatingSettings:MinRating", 0);
        _maxRating = configuration.GetValue<decimal>("RatingSettings:MaxRating", 10);
    }

    public async Task<IReadOnlyList<UserRatingCriterionOption>> GetRequiredCriteriaAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.RatingCriterias
            .Where(c => RequiredCriteriaNames.Contains(c.Name))
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new UserRatingCriterionOption
            {
                CriteriaId = c.CriteriaId,
                Name = c.Name,
                DisplayOrder = c.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<UserRatingSubmissionResult> SubmitRatingAsync(UserRatingSubmissionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!ValidationHelper.IsValidBankCode(request.BankCode))
        {
            _logger.LogWarning("Invalid bank code attempted for rating submission: {BankCode}", request.BankCode);
            return new UserRatingSubmissionResult { Success = false, Error = UserRatingSubmissionError.InvalidBankCode };
        }

        var comment = request.Comment?.Trim();
        if (!string.IsNullOrEmpty(comment) && comment.Length > MaxCommentLength)
        {
            return new UserRatingSubmissionResult { Success = false, Error = UserRatingSubmissionError.CommentTooLong };
        }

        if (!await CheckRateLimitAsync(request.SubmitterIP))
        {
            return new UserRatingSubmissionResult
            {
                Success = false,
                Error = UserRatingSubmissionError.RateLimited,
                RemainingSubmissions = 0
            };
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        var bank = await context.Banks.FirstOrDefaultAsync(b => b.BankCode == request.BankCode);
        if (bank == null)
        {
            _logger.LogWarning("Bank not found for rating submission: {BankCode}", request.BankCode);
            return new UserRatingSubmissionResult { Success = false, Error = UserRatingSubmissionError.BankNotFound };
        }

        var criteria = await context.RatingCriterias
            .Where(c => RequiredCriteriaNames.Contains(c.Name))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        if (criteria.Count != RequiredCriteriaNames.Length)
        {
            _logger.LogError(
                "Rating criteria configuration mismatch. Expected {Expected} criteria, found {Actual}",
                RequiredCriteriaNames.Length,
                criteria.Count);
            return new UserRatingSubmissionResult { Success = false, Error = UserRatingSubmissionError.MissingCriteria };
        }

        if (request.CriteriaRatings.Count != criteria.Count || criteria.Any(c => !request.CriteriaRatings.ContainsKey(c.CriteriaId)))
        {
            return new UserRatingSubmissionResult { Success = false, Error = UserRatingSubmissionError.MissingCriteria };
        }

        if (request.CriteriaRatings.Any(kvp => kvp.Value < _minRating || kvp.Value > _maxRating))
        {
            return new UserRatingSubmissionResult { Success = false, Error = UserRatingSubmissionError.InvalidRatingValue };
        }

        var ratingsByName = criteria.ToDictionary(
            c => c.Name,
            c => request.CriteriaRatings[c.CriteriaId],
            StringComparer.OrdinalIgnoreCase);

        var utcNow = DateTime.UtcNow;
        var submission = new UserRatingSubmission
        {
            BankId = bank.BankId,
            SubmitterIP = NormalizeIpAddress(request.SubmitterIP),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment,
            ServiceRating = ratingsByName["Service"],
            FeesRating = ratingsByName["Fees"],
            ConvenienceRating = ratingsByName["Convenience"],
            DigitalServicesRating = ratingsByName["Digital Services"],
            CustomerSupportRating = ratingsByName["Customer Support"],
            SubmittedDate = utcNow
        };

        context.UserRatingSubmissions.Add(submission);

        foreach (var criterion in criteria)
        {
            context.BankRatings.Add(new BankRating
            {
                BankId = bank.BankId,
                CriteriaId = criterion.CriteriaId,
                UserRatingSubmission = submission,
                RatingValue = request.CriteriaRatings[criterion.CriteriaId],
                RatingDate = utcNow,
                Notes = "User submitted rating"
            });
        }

        await context.SaveChangesAsync();

        var remaining = await GetRemainingSubmissionsAsync(request.SubmitterIP);

        return new UserRatingSubmissionResult
        {
            Success = true,
            RemainingSubmissions = remaining
        };
    }

    public async Task<bool> CheckRateLimitAsync(string? ipAddress)
    {
        var normalizedIp = NormalizeIpAddress(ipAddress);
        if (string.IsNullOrEmpty(normalizedIp))
        {
            return true;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var cutoff = DateTime.UtcNow.AddHours(-RateLimitWindowHours);
        var count = await context.UserRatingSubmissions
            .Where(s => s.SubmitterIP == normalizedIp && s.SubmittedDate >= cutoff)
            .CountAsync();

        return count < MaxSubmissionsPerDay;
    }

    public async Task<int> GetRemainingSubmissionsAsync(string? ipAddress)
    {
        var normalizedIp = NormalizeIpAddress(ipAddress);
        if (string.IsNullOrEmpty(normalizedIp))
        {
            return MaxSubmissionsPerDay;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var cutoff = DateTime.UtcNow.AddHours(-RateLimitWindowHours);
        var count = await context.UserRatingSubmissions
            .Where(s => s.SubmitterIP == normalizedIp && s.SubmittedDate >= cutoff)
            .CountAsync();

        return Math.Max(0, MaxSubmissionsPerDay - count);
    }

    private static string? NormalizeIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return null;
        }

        return ipAddress.Trim();
    }
}
