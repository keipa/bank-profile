using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services
{
   public interface IBankMetricsExtractorService
   {
      Dictionary<string, List<MetricDto>> ExtractMetrics(BankProfile bank);
   }
}