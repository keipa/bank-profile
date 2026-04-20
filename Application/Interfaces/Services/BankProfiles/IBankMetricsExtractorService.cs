using BankProfiles.Web.Domain.BankProfiles;
using BankProfiles.Web.Domain.Common.Metrics;

namespace BankProfiles.Web.Application.Interfaces.Services.BankProfiles;

public interface IBankMetricsExtractorService
{
   Dictionary<string, List<MetricDto>> ExtractMetrics(BankProfile bank);
}