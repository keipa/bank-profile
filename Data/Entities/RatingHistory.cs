namespace BankProfiles.Web.Data.Entities;

public class RatingHistory
{
    public int HistoryId { get; set; }
    public int BankId { get; set; }
    public int CriteriaId { get; set; }
    public decimal OverallRating { get; set; }
    public DateTime RecordedDate { get; set; }

    // Navigation properties
    public Bank Bank { get; set; } = null!;
    public RatingCriteria Criteria { get; set; } = null!;
}
