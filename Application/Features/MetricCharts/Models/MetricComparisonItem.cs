namespace BankProfiles.Web.Application.Features.MetricCharts.Models;

public class MetricComparisonItem
{
    public required string BankCode { get; set; }
    public required string BankName { get; set; }
    public double Value { get; set; }
    public bool IsCurrentBank { get; set; }
}
