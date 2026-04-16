using BankProfiles.Web.Data.Entities;

namespace BankProfiles.Web.Services;

public interface IFeedbackService
{
    Task<bool> SubmitFeedbackAsync(MetricFeedback feedback);
    Task<List<MetricFeedback>> GetFeedbackForBankAsync(int? bankId);
    Task<bool> CheckRateLimitAsync(string ipAddress);
    Task<int> GetRemainingSubmissionsAsync(string ipAddress);
    Task CleanupOldSubmissionsAsync();
}
