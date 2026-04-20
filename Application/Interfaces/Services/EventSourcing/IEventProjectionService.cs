using BankProfiles.Web.Domain.BankProfiles;
using BankProfiles.Web.Infrastructure.Persistence.Entities;

namespace BankProfiles.Web.Application.Interfaces.Services.EventSourcing;

public interface IEventProjectionService
{
   Task<BankProfile?> ProjectBankProfileAsync(string bankCode);
   BankProfile? ProjectFromEvents(List<MetricEvent> events);
}