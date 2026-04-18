using BankProfiles.Web.Data.Entities;

namespace BankProfiles.Web.Services;

public interface IBankOnboardingService
{
    Task<BankOnboardingSubmissionResult> SubmitAsync(BankOnboardingSubmissionRequest request);
    Task<int> GetRemainingSubmissionsAsync(string? ipAddress);
    Task<List<BankOnboardingSubmission>> GetPendingSubmissionsAsync();
    Task<BankOnboardingSubmission?> GetSubmissionByIdAsync(int submissionId);
    Task<BankOnboardingApprovalResult> ApproveSubmissionAsync(BankOnboardingApprovalRequest request);
    Task<bool> RejectSubmissionAsync(int submissionId, string rejectionReason, string? reviewNotes);
}
