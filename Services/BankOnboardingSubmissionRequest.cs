namespace BankProfiles.Web.Services
{
   public sealed class BankOnboardingSubmissionRequest
   {
      public string ProposedBankName { get; init; } = string.Empty;
      public string ProposedCountryCode { get; init; } = string.Empty;
      public string? ProposedWebsiteUrl { get; init; }
      public string? SubmissionNotes { get; init; }
      public string? ContactEmail { get; init; }
      public string? SubmitterIP { get; init; }
   }
}