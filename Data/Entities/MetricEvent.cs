namespace BankProfiles.Web.Data.Entities;

public class MetricEvent
{
    public long EventId { get; set; }
    public required string BankCode { get; set; }
    public required string Country { get; set; }
    public required string MetricName { get; set; }
    public required string MetricValue { get; set; }
    public required string MetricType { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int EventVersion { get; set; } = 1;
    public long EventSequence { get; set; }
}
