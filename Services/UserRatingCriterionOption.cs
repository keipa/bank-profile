namespace BankProfiles.Web.Services
{
   public sealed class UserRatingCriterionOption
   {
      public int CriteriaId { get; init; }
      public string Name { get; init; } = string.Empty;
      public int DisplayOrder { get; init; }
   }
}