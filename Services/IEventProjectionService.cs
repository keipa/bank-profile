using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services
{
   public interface IEventProjectionService
   {
      Task<BankProfile?> ProjectBankProfileAsync(string bankCode);
      BankProfile? ProjectFromEvents(List<MetricEvent> events);
   }
}