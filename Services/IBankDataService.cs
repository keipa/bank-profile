using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services
{
   public interface IBankDataService
   {
      Task<BankProfile?> GetBankByCodeAsync(string bankCode);
      Task<List<BankProfile>> GetAllBanksAsync();
      Task RefreshCacheAsync();
   }
}