using BankProfiles.Web.Infrastructure.Persistence.Entities;

namespace BankProfiles.Web.Application.Interfaces.Repositories.EventSourcing;

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
   Task<List<MetricEvent>> GetLatestEventByMetricAcrossBanksAsync(string metricName);
}