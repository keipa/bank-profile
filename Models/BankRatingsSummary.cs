namespace BankProfiles.Web.Models
{
   public class BankRatingsSummary
   {
      public required string BankCode { get; set; }
      public string? CountryCode { get; set; }
      public required string BankName { get; set; }
      public decimal OverallRating { get; set; }
      public List<RatingData>? CriteriaRatings { get; set; }
   }
}