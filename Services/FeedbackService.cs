using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BankProfiles.Web.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<FeedbackService> _logger;
    private const int MaxSubmissionsPerDay = 10;
    private const int RateLimitWindowHours = 24;
    private const int CleanupDays = 7;

    public FeedbackService(
        IDbContextFactory<BankDbContext> contextFactory,
        ILogger<FeedbackService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<bool> SubmitFeedbackAsync(MetricFeedback feedback)
    {
        try
        {
            // Validate input
            if (!ValidateInput(feedback))
            {
                _logger.LogWarning("Invalid feedback submission from IP: {IP}", feedback.SubmitterIP);
                return false;
            }

            // Check rate limit
            if (!string.IsNullOrEmpty(feedback.SubmitterIP))
            {
                var canSubmit = await CheckRateLimitAsync(feedback.SubmitterIP);
                if (!canSubmit)
                {
                    _logger.LogWarning("Rate limit exceeded for IP: {IP}", feedback.SubmitterIP);
                    return false;
                }
            }

            // Sanitize input
            SanitizeInput(feedback);

            await using var context = await _contextFactory.CreateDbContextAsync();

            // Save feedback
            context.MetricFeedbacks.Add(feedback);

            // Track submission for rate limiting
            if (!string.IsNullOrEmpty(feedback.SubmitterIP))
            {
                context.FeedbackSubmissions.Add(new FeedbackSubmission
                {
                    SubmitterIP = feedback.SubmitterIP,
                    SubmissionDate = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Feedback submitted successfully from IP: {IP}, Bank: {BankId}, Category: {Category}",
                feedback.SubmitterIP, feedback.BankId, feedback.MetricCategory);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback from IP: {IP}", feedback.SubmitterIP);
            return false;
        }
    }

    public async Task<List<MetricFeedback>> GetFeedbackForBankAsync(int? bankId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.MetricFeedbacks
                .Include(f => f.Bank)
                .AsQueryable();

            if (bankId.HasValue)
            {
                query = query.Where(f => f.BankId == bankId.Value);
            }

            return await query
                .OrderByDescending(f => f.SubmittedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feedback for bank: {BankId}", bankId);
            return new List<MetricFeedback>();
        }
    }

    public async Task<bool> CheckRateLimitAsync(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            return true;
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var cutoffTime = DateTime.UtcNow.AddHours(-RateLimitWindowHours);

            var submissionCount = await context.FeedbackSubmissions
                .Where(fs => fs.SubmitterIP == ipAddress && fs.SubmissionDate >= cutoffTime)
                .CountAsync();

            return submissionCount < MaxSubmissionsPerDay;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for IP: {IP}", ipAddress);
            // Allow submission on error to avoid blocking legitimate users
            return true;
        }
    }

    public async Task<int> GetRemainingSubmissionsAsync(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            return MaxSubmissionsPerDay;
        }

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var cutoffTime = DateTime.UtcNow.AddHours(-RateLimitWindowHours);

            var submissionCount = await context.FeedbackSubmissions
                .Where(fs => fs.SubmitterIP == ipAddress && fs.SubmissionDate >= cutoffTime)
                .CountAsync();

            return Math.Max(0, MaxSubmissionsPerDay - submissionCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining submissions for IP: {IP}", ipAddress);
            return MaxSubmissionsPerDay;
        }
    }

    public async Task CleanupOldSubmissionsAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var cutoffDate = DateTime.UtcNow.AddDays(-CleanupDays);

            var oldSubmissions = await context.FeedbackSubmissions
                .Where(fs => fs.SubmissionDate < cutoffDate)
                .ToListAsync();

            if (oldSubmissions.Count > 0)
            {
                context.FeedbackSubmissions.RemoveRange(oldSubmissions);
                await context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} old feedback submissions", oldSubmissions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old submissions");
        }
    }

    private bool ValidateInput(MetricFeedback feedback)
    {
        if (string.IsNullOrWhiteSpace(feedback.MetricCategory) ||
            string.IsNullOrWhiteSpace(feedback.MetricName) ||
            string.IsNullOrWhiteSpace(feedback.Explanation))
        {
            return false;
        }

        if (feedback.MetricCategory.Length > 100 ||
            feedback.MetricName.Length > 200 ||
            feedback.Explanation.Length > 2000 ||
            (feedback.CurrentValue?.Length ?? 0) > 500 ||
            (feedback.SuggestedValue?.Length ?? 0) > 500)
        {
            return false;
        }

        return true;
    }

    private void SanitizeInput(MetricFeedback feedback)
    {
        // Trim all string inputs
        feedback.MetricCategory = feedback.MetricCategory.Trim();
        feedback.MetricName = feedback.MetricName.Trim();
        feedback.Explanation = feedback.Explanation.Trim();

        if (!string.IsNullOrEmpty(feedback.CurrentValue))
        {
            feedback.CurrentValue = feedback.CurrentValue.Trim();
            feedback.CurrentValue = SanitizeHtml(feedback.CurrentValue);
        }

        if (!string.IsNullOrEmpty(feedback.SuggestedValue))
        {
            feedback.SuggestedValue = feedback.SuggestedValue.Trim();
            feedback.SuggestedValue = SanitizeHtml(feedback.SuggestedValue);
        }

        feedback.Explanation = SanitizeHtml(feedback.Explanation);
    }

    private string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Remove script tags and their content
        input = Regex.Replace(input, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Remove all HTML tags
        input = Regex.Replace(input, @"<[^>]+>", string.Empty);

        // Decode HTML entities to prevent double-encoding
        input = System.Net.WebUtility.HtmlDecode(input);

        return input;
    }
}
