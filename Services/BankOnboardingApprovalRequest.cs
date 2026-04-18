namespace BankProfiles.Web.Services
{
   public sealed class BankOnboardingApprovalRequest
   {
      public int SubmissionId { get; init; }
      public string BankCode { get; init; } = string.Empty;
      public string BankName { get; init; } = string.Empty;
      public string LegalName { get; init; } = string.Empty;
      public string Status { get; init; } = "active";
      public string CountryCode { get; init; } = string.Empty;
      public string? Jurisdiction { get; init; }
      public string? Description { get; init; }
      public string? WebsiteUrl { get; init; }
      public string? ContactEmail { get; init; }
      public decimal DefaultCriteriaRating { get; init; } = 7.5m;
      public string? ReviewNotes { get; init; }
   }
}