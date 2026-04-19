using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BankProfiles.Web.Services;

public class FeedbackService : IFeedbackService
{
    private const int MaxSubmissionsPerDay = 10;
    private const int RateLimitWindowHours = 24;
    private const int CleanupDays = 7;
    private const int MaxReviewerLength = 100;
    private const int MaxReviewNotesLength = 2000;
    private const int MaxEventCommentLength = 1000;
    private const string AllBanksCacheKey = "all_banks";

    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly IEventStoreService _eventStoreService;
    private readonly IEventMigrationService _eventMigrationService;
    private readonly IBankDataService _bankDataService;
    private readonly ICacheManager _cacheManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        IDbContextFactory<BankDbContext> contextFactory,
        IEventStoreService eventStoreService,
        IEventMigrationService eventMigrationService,
        IBankDataService bankDataService,
        ICacheManager cacheManager,
        IWebHostEnvironment environment,
        ILogger<FeedbackService> logger)
    {
        _contextFactory = contextFactory;
        _eventStoreService = eventStoreService;
        _eventMigrationService = eventMigrationService;
        _bankDataService = bankDataService;
        _cacheManager = cacheManager;
        _environment = environment;
        _logger = logger;
    }

    public async Task<bool> SubmitFeedbackAsync(MetricFeedback feedback)
    {
        try
        {
            if (!ValidateInput(feedback))
            {
                _logger.LogWarning("Invalid feedback submission from IP: {IP}", feedback.SubmitterIP);
                return false;
            }

            SanitizeInput(feedback);

            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var now = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(feedback.SubmitterIP))
            {
                var cutoffTime = now.AddHours(-RateLimitWindowHours);
                var submissionCount = await context.FeedbackSubmissions
                    .Where(fs => fs.SubmitterIP == feedback.SubmitterIP && fs.SubmissionDate >= cutoffTime)
                    .CountAsync();

                if (submissionCount >= MaxSubmissionsPerDay)
                {
                    _logger.LogWarning("Rate limit exceeded for IP: {IP}", feedback.SubmitterIP);
                    return false;
                }
            }

            if (!await ResolveBankIdentifiersAsync(context, feedback))
            {
                _logger.LogWarning("Feedback submission contains invalid bank code: {BankCode}", feedback.BankCode);
                return false;
            }

            if (!ResolveMetricPath(feedback))
            {
                _logger.LogWarning(
                    "Feedback submission contains unknown metric mapping. Category: {Category}, Metric: {MetricName}",
                    feedback.MetricCategory,
                    feedback.MetricName);
                return false;
            }

            feedback.Status = MetricFeedbackStatuses.Pending;
            feedback.ReviewedBy = null;
            feedback.ReviewedDate = null;
            feedback.ReviewNotes = null;
            feedback.AppliedEventId = null;

            context.MetricFeedbacks.Add(feedback);

            if (!string.IsNullOrEmpty(feedback.SubmitterIP))
            {
                context.FeedbackSubmissions.Add(new FeedbackSubmission
                {
                    SubmitterIP = feedback.SubmitterIP,
                    SubmissionDate = now
                });
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Feedback submitted successfully from IP: {IP}, Bank: {BankCode}, MetricPath: {MetricPath}",
                feedback.SubmitterIP,
                feedback.BankCode,
                feedback.MetricPath);

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
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.MetricFeedbacks.AsQueryable();

        if (bankId.HasValue)
        {
            query = query.Where(f => f.BankId == bankId.Value);
        }

        return await query
            .OrderByDescending(f => f.SubmittedDate)
            .ToListAsync();
    }

    public async Task<List<MetricFeedback>> GetFeedbackByStatusAsync(string? status = null, int take = 200)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var normalizedTake = Math.Clamp(take, 1, 1000);
        var query = context.MetricFeedbacks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            query = query.Where(f => f.Status == normalizedStatus);
        }

        return await query
            .OrderBy(f => f.Status == MetricFeedbackStatuses.Pending ? 0 : 1)
            .ThenByDescending(f => f.SubmittedDate)
            .Take(normalizedTake)
            .ToListAsync();
    }

    public async Task<FeedbackModerationResult> ApproveFeedbackAsync(int feedbackId, string reviewedBy, string? reviewNotes)
    {
        if (!_environment.IsDevelopment())
        {
            return new FeedbackModerationResult
            {
                Success = false,
                Error = FeedbackModerationError.ModerationDisabled,
                ErrorMessage = "Feedback moderation is available only in development mode."
            };
        }

        if (feedbackId <= 0)
        {
            return new FeedbackModerationResult
            {
                Success = false,
                Error = FeedbackModerationError.NotFound,
                ErrorMessage = "Feedback record not found."
            };
        }

        var reviewer = NormalizeReviewer(reviewedBy);
        var normalizedReviewNotes = NormalizeReviewNotes(reviewNotes);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var feedback = await context.MetricFeedbacks.FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
            if (feedback == null)
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.NotFound,
                    ErrorMessage = "Feedback record not found."
                };
            }

            if (!string.Equals(feedback.Status, MetricFeedbackStatuses.Pending, StringComparison.OrdinalIgnoreCase))
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.AlreadyReviewed,
                    ErrorMessage = "Only pending feedback can be approved."
                };
            }

            if (string.IsNullOrWhiteSpace(feedback.BankCode))
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.MissingBankCode,
                    ErrorMessage = "Feedback has no target bank code."
                };
            }

            var bankCode = feedback.BankCode.Trim();
            if (!ValidationHelper.IsValidBankCode(bankCode))
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.InvalidBankCode,
                    ErrorMessage = "Feedback has an invalid bank code."
                };
            }

            var metricPath = string.IsNullOrWhiteSpace(feedback.MetricPath) ? null : feedback.MetricPath.Trim();
            if (string.IsNullOrWhiteSpace(metricPath))
            {
                if (!FeedbackMetricCatalog.TryResolveMetricPath(feedback.MetricCategory, feedback.MetricName, out var resolvedMetricPath))
                {
                    return new FeedbackModerationResult
                    {
                        Success = false,
                        Error = FeedbackModerationError.MissingMetricPath,
                        ErrorMessage = "Feedback has no resolvable metric path."
                    };
                }

                metricPath = resolvedMetricPath;
                feedback.MetricPath = resolvedMetricPath;
            }

            if (!EventProjectionService.TryGetMetricPropertyType(metricPath, out var targetType) || targetType == null)
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.InvalidMetricPath,
                    ErrorMessage = "Feedback metric path is not supported by projection."
                };
            }

            if (string.IsNullOrWhiteSpace(feedback.SuggestedValue))
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.MissingSuggestedValue,
                    ErrorMessage = "Suggested value is required for approval."
                };
            }

            if (!await _eventStoreService.HasEventsAsync(bankCode))
            {
                var migrationResult = await _eventMigrationService.MigrateSingleBankAsync(bankCode);
                if (migrationResult.Errors.Count > 0)
                {
                    return new FeedbackModerationResult
                    {
                        Success = false,
                        Error = FeedbackModerationError.MigrationFailed,
                        ErrorMessage = migrationResult.Errors[0]
                    };
                }
            }

            var bankProfile = await _bankDataService.GetBankByCodeAsync(bankCode);
            if (bankProfile == null)
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.InvalidBankCode,
                    ErrorMessage = "Target bank could not be loaded."
                };
            }

            if (!TrySerializeSuggestedValue(feedback.SuggestedValue, targetType, metricPath, out var serializedValue, out var metricType, out var parseError))
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.InvalidSuggestedValue,
                    ErrorMessage = parseError
                };
            }

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var nextSequence = (await context.MetricEvents
                .Where(e => e.BankCode == bankCode)
                .MaxAsync(e => (long?)e.EventSequence) ?? 0) + 1;

            var appendedEvent = new MetricEvent
            {
                BankCode = bankCode,
                Country = string.IsNullOrWhiteSpace(bankProfile.HeadquartersCountry)
                    ? "unknown"
                    : bankProfile.HeadquartersCountry,
                MetricName = metricPath,
                MetricValue = serializedValue,
                MetricType = metricType,
                Comment = BuildApprovalComment(feedback),
                EventVersion = 1,
                EventSequence = nextSequence,
                CreatedDate = DateTime.UtcNow
            };

            context.MetricEvents.Add(appendedEvent);

            feedback.Status = MetricFeedbackStatuses.Approved;
            feedback.ReviewedBy = reviewer;
            feedback.ReviewedDate = DateTime.UtcNow;
            feedback.ReviewNotes = normalizedReviewNotes;
            feedback.AppliedEventId = null;

            await context.SaveChangesAsync();
            feedback.AppliedEventId = appendedEvent.EventId;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            InvalidateBankCache(bankCode);

            _logger.LogInformation(
                "Feedback {FeedbackId} approved by {Reviewer}. Applied event {EventId} for {BankCode} ({MetricPath}).",
                feedback.FeedbackId,
                reviewer,
                appendedEvent.EventId,
                bankCode,
                metricPath);

            return new FeedbackModerationResult
            {
                Success = true,
                AppliedEventId = appendedEvent.EventId
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Feedback {FeedbackId} was already reviewed by another moderator.", feedbackId);
            return new FeedbackModerationResult
            {
                Success = false,
                Error = FeedbackModerationError.AlreadyReviewed,
                ErrorMessage = "Feedback was already reviewed by another moderator."
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while approving feedback {FeedbackId}.", feedbackId);
            return new FeedbackModerationResult
            {
                Success = false,
                Error = FeedbackModerationError.OperationFailed,
                ErrorMessage = "Unable to approve feedback due to a database conflict. Please retry."
            };
        }
    }

    public async Task<FeedbackModerationResult> RejectFeedbackAsync(int feedbackId, string reviewedBy, string? reviewNotes)
    {
        if (!_environment.IsDevelopment())
        {
            return new FeedbackModerationResult
            {
                Success = false,
                Error = FeedbackModerationError.ModerationDisabled,
                ErrorMessage = "Feedback moderation is available only in development mode."
            };
        }

        if (feedbackId <= 0)
        {
            return new FeedbackModerationResult
            {
                Success = false,
                Error = FeedbackModerationError.NotFound,
                ErrorMessage = "Feedback record not found."
            };
        }

        var reviewer = NormalizeReviewer(reviewedBy);
        var normalizedReviewNotes = NormalizeReviewNotes(reviewNotes);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var feedback = await context.MetricFeedbacks.FirstOrDefaultAsync(f => f.FeedbackId == feedbackId);
            if (feedback == null)
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.NotFound,
                    ErrorMessage = "Feedback record not found."
                };
            }

            if (!string.Equals(feedback.Status, MetricFeedbackStatuses.Pending, StringComparison.OrdinalIgnoreCase))
            {
                return new FeedbackModerationResult
                {
                    Success = false,
                    Error = FeedbackModerationError.AlreadyReviewed,
                    ErrorMessage = "Only pending feedback can be rejected."
                };
            }

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            feedback.Status = MetricFeedbackStatuses.Rejected;
            feedback.ReviewedBy = reviewer;
            feedback.ReviewedDate = DateTime.UtcNow;
            feedback.ReviewNotes = normalizedReviewNotes;
            feedback.AppliedEventId = null;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Feedback {FeedbackId} rejected by {Reviewer}.", feedback.FeedbackId, reviewer);

            return new FeedbackModerationResult { Success = true };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Feedback {FeedbackId} was already reviewed by another moderator.", feedbackId);
            return new FeedbackModerationResult
            {
                Success = false,
                Error = FeedbackModerationError.AlreadyReviewed,
                ErrorMessage = "Feedback was already reviewed by another moderator."
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while rejecting feedback {FeedbackId}.", feedbackId);
            return new FeedbackModerationResult
            {
                Success = false,
                Error = FeedbackModerationError.OperationFailed,
                ErrorMessage = "Unable to reject feedback due to a database conflict. Please retry."
            };
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
            return false;
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
            return 0;
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

    private static bool ValidateInput(MetricFeedback feedback)
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
            (feedback.SuggestedValue?.Length ?? 0) > 500 ||
            (feedback.BankCode?.Length ?? 0) > 50 ||
            (feedback.MetricPath?.Length ?? 0) > 200)
        {
            return false;
        }

        return true;
    }

    private static void SanitizeInput(MetricFeedback feedback)
    {
        feedback.MetricCategory = feedback.MetricCategory.Trim();
        feedback.MetricName = feedback.MetricName.Trim();
        feedback.Explanation = SanitizeHtml(feedback.Explanation.Trim());
        feedback.BankCode = string.IsNullOrWhiteSpace(feedback.BankCode) ? null : feedback.BankCode.Trim();
        feedback.MetricPath = string.IsNullOrWhiteSpace(feedback.MetricPath) ? null : feedback.MetricPath.Trim();

        if (!string.IsNullOrEmpty(feedback.CurrentValue))
        {
            feedback.CurrentValue = SanitizeHtml(feedback.CurrentValue.Trim());
        }

        if (!string.IsNullOrEmpty(feedback.SuggestedValue))
        {
            feedback.SuggestedValue = SanitizeHtml(feedback.SuggestedValue.Trim());
        }
    }

    private static string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        input = Regex.Replace(input, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        input = Regex.Replace(input, @"<[^>]+>", string.Empty);
        input = System.Net.WebUtility.HtmlDecode(input);

        return input;
    }

    private static bool ResolveMetricPath(MetricFeedback feedback)
    {
        if (!string.IsNullOrWhiteSpace(feedback.MetricPath))
        {
            return EventProjectionService.TryGetMetricPropertyType(feedback.MetricPath, out _);
        }

        if (!FeedbackMetricCatalog.TryResolveMetricPath(feedback.MetricCategory, feedback.MetricName, out var metricPath))
        {
            return false;
        }

        feedback.MetricPath = metricPath;
        return true;
    }

    private static string NormalizeReviewer(string reviewedBy)
    {
        var reviewer = string.IsNullOrWhiteSpace(reviewedBy) ? "admin" : reviewedBy.Trim();
        return reviewer.Length <= MaxReviewerLength ? reviewer : reviewer[..MaxReviewerLength];
    }

    private static string? NormalizeReviewNotes(string? reviewNotes)
    {
        if (string.IsNullOrWhiteSpace(reviewNotes))
        {
            return null;
        }

        var normalizedNotes = reviewNotes.Trim();
        return normalizedNotes.Length <= MaxReviewNotesLength
            ? normalizedNotes
            : normalizedNotes[..MaxReviewNotesLength];
    }

    private static string BuildApprovalComment(MetricFeedback feedback)
    {
        var prefix = $"Approved feedback #{feedback.FeedbackId}: ";
        var maxExplanationLength = Math.Max(0, MaxEventCommentLength - prefix.Length);
        var explanation = feedback.Explanation.Length <= maxExplanationLength
            ? feedback.Explanation
            : feedback.Explanation[..maxExplanationLength];
        return prefix + explanation;
    }

    private static bool TrySerializeSuggestedValue(
        string suggestedValue,
        Type targetType,
        string metricPath,
        out string serializedValue,
        out string metricType,
        out string errorMessage)
    {
        serializedValue = string.Empty;
        metricType = "Text";
        errorMessage = string.Empty;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var trimmedValue = suggestedValue.Trim();

        if (underlyingType == typeof(string))
        {
            serializedValue = JsonSerializer.Serialize(trimmedValue);
            metricType = "Text";
            return true;
        }

        if (underlyingType == typeof(bool))
        {
            if (!TryParseBoolean(trimmedValue, out var parsedBool))
            {
                errorMessage = "Suggested value must be a boolean (true/false).";
                return false;
            }

            serializedValue = JsonSerializer.Serialize(parsedBool);
            metricType = "Boolean";
            return true;
        }

        if (underlyingType == typeof(int))
        {
            if (!int.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedInt) &&
                !int.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out parsedInt))
            {
                errorMessage = "Suggested value must be an integer.";
                return false;
            }

            serializedValue = JsonSerializer.Serialize(parsedInt);
            metricType = "Numeric";
            return true;
        }

        if (underlyingType == typeof(long))
        {
            if (!long.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedLong) &&
                !long.TryParse(trimmedValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out parsedLong))
            {
                errorMessage = "Suggested value must be a whole number.";
                return false;
            }

            serializedValue = JsonSerializer.Serialize(parsedLong);
            metricType = "Numeric";
            return true;
        }

        if (underlyingType == typeof(double))
        {
            if (!double.TryParse(trimmedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDouble) &&
                !double.TryParse(trimmedValue, NumberStyles.Float, CultureInfo.CurrentCulture, out parsedDouble))
            {
                errorMessage = "Suggested value must be a number.";
                return false;
            }

            serializedValue = JsonSerializer.Serialize(parsedDouble);
            metricType = ResolveNumericMetricType(metricPath);
            return true;
        }

        if (underlyingType == typeof(decimal))
        {
            if (!decimal.TryParse(trimmedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDecimal) &&
                !decimal.TryParse(trimmedValue, NumberStyles.Float, CultureInfo.CurrentCulture, out parsedDecimal))
            {
                errorMessage = "Suggested value must be a decimal number.";
                return false;
            }

            serializedValue = JsonSerializer.Serialize(parsedDecimal);
            metricType = ResolveNumericMetricType(metricPath);
            return true;
        }

        if (underlyingType == typeof(DateTime))
        {
            if (!DateTime.TryParse(trimmedValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDateTime) &&
                !DateTime.TryParse(trimmedValue, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out parsedDateTime))
            {
                errorMessage = "Suggested value must be a valid date or date-time.";
                return false;
            }

            serializedValue = JsonSerializer.Serialize(parsedDateTime);
            metricType = "Text";
            return true;
        }

        if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = underlyingType.GetGenericArguments()[0];
            if (elementType != typeof(string))
            {
                errorMessage = "Only string lists are supported for metric updates.";
                return false;
            }

            if (TryParseStringList(trimmedValue, out var parsedList))
            {
                serializedValue = JsonSerializer.Serialize(parsedList);
                metricType = "List";
                return true;
            }

            errorMessage = "Suggested value must be a JSON array or comma-separated list.";
            return false;
        }

        errorMessage = $"Metric type '{underlyingType.Name}' is not supported for updates.";
        return false;
    }

    private static bool TryParseBoolean(string value, out bool result)
    {
        if (bool.TryParse(value, out result))
        {
            return true;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "1":
            case "yes":
            case "y":
                result = true;
                return true;
            case "0":
            case "no":
            case "n":
                result = false;
                return true;
            default:
                result = false;
                return false;
        }
    }

    private static bool TryParseStringList(string value, out List<string> items)
    {
        items = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value.StartsWith("[", StringComparison.Ordinal))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(value);
                if (parsed == null)
                {
                    return false;
                }

                items = parsed
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Select(item => item.Trim())
                    .ToList();
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        items = value
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .ToList();

        return items.Count > 0;
    }

    private static string ResolveNumericMetricType(string metricPath)
    {
        if (metricPath.Contains("Percent", StringComparison.OrdinalIgnoreCase))
        {
            return "Percentage";
        }

        if (metricPath.Contains("Fee", StringComparison.OrdinalIgnoreCase) ||
            metricPath.Contains("Maintenance", StringComparison.OrdinalIgnoreCase) ||
            metricPath.Contains("Surcharge", StringComparison.OrdinalIgnoreCase))
        {
            return "Currency";
        }

        return "Numeric";
    }

    private async Task<bool> ResolveBankIdentifiersAsync(BankDbContext context, MetricFeedback feedback)
    {
        if (!string.IsNullOrWhiteSpace(feedback.BankCode))
        {
            if (!ValidationHelper.IsValidBankCode(feedback.BankCode))
            {
                return false;
            }

            var bank = await context.Banks
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BankCode == feedback.BankCode);
            if (bank == null)
            {
                return false;
            }

            feedback.BankId = bank.BankId;
            feedback.BankCode = bank.BankCode;
            return true;
        }

        if (feedback.BankId.HasValue)
        {
            var bank = await context.Banks
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BankId == feedback.BankId.Value);
            if (bank == null)
            {
                return false;
            }

            feedback.BankCode = bank.BankCode;
        }

        return true;
    }

    private void InvalidateBankCache(string bankCode)
    {
        _cacheManager.Remove($"bank_{bankCode}");
        _cacheManager.Remove(AllBanksCacheKey);
    }
}
