namespace BankProfiles.Web.Data.Entities;

public class RatingCriteria
{
    public int CriteriaId { get; set; }
    public required string Name { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation properties
    public ICollection<BankRating> BankRatings { get; set; } = new List<BankRating>();
    public ICollection<RatingHistory> RatingHistories { get; set; } = new List<RatingHistory>();
}
