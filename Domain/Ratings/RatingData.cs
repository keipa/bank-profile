namespace BankProfiles.Web.Domain.Ratings;

public class RatingData
{
    public required string CriteriaName { get; set; }
    public decimal CurrentRating { get; set; }
    public List<RatingHistoryPoint>? History { get; set; }
}