using System.ComponentModel.DataAnnotations;

namespace BankProfiles.Web.Data.Entities;

public class MetricFeedback
{
    public int FeedbackId { get; set; }

    public int? BankId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string MetricCategory { get; set; }

    [Required]
    [MaxLength(200)]
    public required string MetricName { get; set; }

    [MaxLength(500)]
    public string? CurrentValue { get; set; }

    [MaxLength(500)]
    public string? SuggestedValue { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Explanation { get; set; }

    [MaxLength(45)]
    public string? SubmitterIP { get; set; }

    public DateTime SubmittedDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(1000)]
    public string? ReviewNotes { get; set; }

    // Navigation properties
    public Bank? Bank { get; set; }
}
