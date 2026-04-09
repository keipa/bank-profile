namespace BankProfiles.Web.Models;

public class RatingData
{
    public required string CriteriaName { get; set; }
    public decimal CurrentRating { get; set; }
    public List<RatingHistoryPoint>? History { get; set; }
}

public class RatingHistoryPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}

public class BankRatingsSummary
{
    public required string BankCode { get; set; }
    public string? CountryCode { get; set; }
    public required string BankName { get; set; }
    public decimal OverallRating { get; set; }
    public List<RatingData>? CriteriaRatings { get; set; }
}
