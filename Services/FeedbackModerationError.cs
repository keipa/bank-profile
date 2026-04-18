namespace BankProfiles.Web.Services
{
   public enum FeedbackModerationError
   {
      None = 0,
      NotFound = 1,
      AlreadyReviewed = 2,
      MissingBankCode = 3,
      InvalidBankCode = 4,
      MissingMetricPath = 5,
      InvalidMetricPath = 6,
      MissingSuggestedValue = 7,
      InvalidSuggestedValue = 8,
      MigrationFailed = 9,
      ModerationDisabled = 10,
      OperationFailed = 11
   }
}