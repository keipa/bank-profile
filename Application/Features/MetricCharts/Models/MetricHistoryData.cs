namespace BankProfiles.Web.Application.Features.MetricCharts.Models;

public class MetricHistoryData
{
    public required string MetricName { get; set; }
    public required List<MetricHistoryPoint> Points { get; set; }
    public bool HasData => Points.Count > 0;
}
