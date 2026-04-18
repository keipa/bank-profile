using System.ComponentModel.DataAnnotations;

namespace BankProfiles.Web.Data.Entities;

public class UserRatingSubmission
{
    public int SubmissionId { get; set; }

    public int BankId { get; set; }

    [MaxLength(45)]
    public string? SubmitterIP { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public decimal ServiceRating { get; set; }
    public decimal FeesRating { get; set; }
    public decimal ConvenienceRating { get; set; }
    public decimal DigitalServicesRating { get; set; }
    public decimal CustomerSupportRating { get; set; }

    public DateTime SubmittedDate { get; set; }

    public Bank Bank { get; set; } = null!;
    public ICollection<BankRating> AppliedRatings { get; set; } = new List<BankRating>();
}
