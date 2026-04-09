namespace BankProfiles.Web.Data.Entities;

public class Bank
{
    public int BankId { get; set; }
    public required string BankCode { get; set; }
    public long ViewCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastViewedDate { get; set; }

    // Navigation properties
    public ICollection<BankRating> BankRatings { get; set; } = new List<BankRating>();
    public ICollection<RatingHistory> RatingHistories { get; set; } = new List<RatingHistory>();
    public ICollection<ViewHistory> ViewHistories { get; set; } = new List<ViewHistory>();
}
