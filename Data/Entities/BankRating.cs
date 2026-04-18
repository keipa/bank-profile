namespace BankProfiles.Web.Data.Entities;

public class BankRating
{
    public int RatingId { get; set; }
    public int BankId { get; set; }
    public int CriteriaId { get; set; }
    public int? UserRatingSubmissionId { get; set; }
    public decimal RatingValue { get; set; }
    public DateTime RatingDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Bank Bank { get; set; } = null!;
    public RatingCriteria Criteria { get; set; } = null!;
    public UserRatingSubmission? UserRatingSubmission { get; set; }
}
