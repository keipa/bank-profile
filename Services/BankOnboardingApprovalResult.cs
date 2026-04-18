namespace BankProfiles.Web.Services
{
   public sealed class BankOnboardingApprovalResult
   {
      public bool Success { get; init; }
      public string? ErrorMessage { get; init; }
      public string? BankCode { get; init; }
   }
}