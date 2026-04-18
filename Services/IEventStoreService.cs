using BankProfiles.Web.Data.Entities;

namespace BankProfiles.Web.Services
{
   public interface IEventStoreService
   {
      Task<MetricEvent> AppendEventAsync(MetricEvent evt);
      Task<List<MetricEvent>> AppendEventsAsync(List<MetricEvent> events);
      Task<List<MetricEvent>> GetEventsForBankAsync(string bankCode);
      Task<List<MetricEvent>> GetEventsByMetricAsync(string bankCode, string metricName);
      Task<List<MetricEvent>> GetEventsInRangeAsync(string bankCode, DateTime from, DateTime to);
      Task<List<string>> GetAllBankCodesAsync();
      Task<long> GetLatestSequenceAsync(string bankCode);
      Task<bool> HasEventsAsync(string bankCode);
   }
}