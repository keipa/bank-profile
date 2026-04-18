using System.ComponentModel.DataAnnotations;

namespace BankProfiles.Web.Data.Entities;

public class BankOnboardingSubmission
{
    public int SubmissionId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string ProposedBankName { get; set; }

    [Required]
    [MaxLength(10)]
    public required string ProposedCountryCode { get; set; }

    [MaxLength(300)]
    public string? ProposedWebsiteUrl { get; set; }

    [MaxLength(2000)]
    public string? SubmissionNotes { get; set; }

    [MaxLength(320)]
    public string? ContactEmail { get; set; }

    [MaxLength(45)]
    public string? SubmitterIP { get; set; }

    public DateTime SubmittedDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(50)]
    public string? ApprovedBankCode { get; set; }

    public DateTime? ReviewedDate { get; set; }

    [MaxLength(2000)]
    public string? ReviewNotes { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }
}
