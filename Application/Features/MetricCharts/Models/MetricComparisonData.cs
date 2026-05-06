namespace BankProfiles.Web.Application.Features.MetricCharts.Models;

public class MetricComparisonData
{
    public required string MetricName { get; set; }
    public required List<MetricComparisonItem> Items { get; set; }
    public bool HasData => Items.Count > 0;
}
