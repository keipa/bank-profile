namespace BankProfiles.Web.Services
{
   public sealed class BankOnboardingSubmissionResult
   {
      public bool Success { get; init; }
      public string? ErrorMessage { get; init; }
      public int RemainingSubmissions { get; init; }
      public int? SubmissionId { get; init; }
   }
}