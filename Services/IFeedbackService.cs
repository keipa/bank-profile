using BankProfiles.Web.Data.Entities;

namespace BankProfiles.Web.Services;

public interface IFeedbackService
{
    Task<bool> SubmitFeedbackAsync(MetricFeedback feedback);
    Task<List<MetricFeedback>> GetFeedbackForBankAsync(int? bankId);
    Task<List<MetricFeedback>> GetFeedbackByStatusAsync(string? status = null, int take = 200);
    Task<FeedbackModerationResult> ApproveFeedbackAsync(int feedbackId, string reviewedBy, string? reviewNotes);
    Task<FeedbackModerationResult> RejectFeedbackAsync(int feedbackId, string reviewedBy, string? reviewNotes);
    Task<bool> CheckRateLimitAsync(string ipAddress);
    Task<int> GetRemainingSubmissionsAsync(string ipAddress);
    Task CleanupOldSubmissionsAsync();
}
