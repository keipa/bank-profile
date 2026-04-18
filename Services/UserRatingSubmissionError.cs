namespace BankProfiles.Web.Services
{
   public enum UserRatingSubmissionError
   {
      None = 0,
      InvalidBankCode = 1,
      BankNotFound = 2,
      MissingCriteria = 3,
      InvalidRatingValue = 4,
      CommentTooLong = 5,
      RateLimited = 6
   }
}