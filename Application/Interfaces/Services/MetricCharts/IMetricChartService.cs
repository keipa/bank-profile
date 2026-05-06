using BankProfiles.Web.Application.Features.MetricCharts.Models;

namespace BankProfiles.Web.Application.Interfaces.Services.MetricCharts;

public interface IMetricChartService
{
    Task<MetricHistoryData?> GetMetricHistoryAsync(string bankCode, string metricKey);
    Task<MetricComparisonData?> GetMetricComparisonAsync(string bankCode, string metricKey);
    bool IsChartableMetric(string metricKey);
}
