namespace BankProfiles.Web.Application.Features.Onboarding.Models;

public sealed class BankOnboardingApprovalResult
{
   public bool Success { get; init; }
   public string? ErrorMessage { get; init; }
   public string? BankCode { get; init; }
}