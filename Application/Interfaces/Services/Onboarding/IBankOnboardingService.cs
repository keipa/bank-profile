using BankProfiles.Web.Application.Features.Onboarding.Models;
using BankProfiles.Web.Infrastructure.Persistence.Entities;

namespace BankProfiles.Web.Application.Interfaces.Services.Onboarding;

public interface IBankOnboardingService
{
    Task<BankOnboardingSubmissionResult> SubmitAsync(BankOnboardingSubmissionRequest request);
    Task<int> GetRemainingSubmissionsAsync(string? ipAddress);
    Task<List<BankOnboardingSubmission>> GetPendingSubmissionsAsync();
    Task<BankOnboardingSubmission?> GetSubmissionByIdAsync(int submissionId);
    Task<BankOnboardingApprovalResult> ApproveSubmissionAsync(BankOnboardingApprovalRequest request);
    Task<bool> RejectSubmissionAsync(int submissionId, string rejectionReason, string? reviewNotes);
}
