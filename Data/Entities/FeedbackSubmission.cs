using System.ComponentModel.DataAnnotations;

namespace BankProfiles.Web.Data.Entities;

public class FeedbackSubmission
{
    public int SubmissionId { get; set; }

    [Required]
    [MaxLength(45)]
    public required string SubmitterIP { get; set; }

    public DateTime SubmissionDate { get; set; }
}
