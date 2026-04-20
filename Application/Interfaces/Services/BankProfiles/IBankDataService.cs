using BankProfiles.Web.Domain.BankProfiles;

namespace BankProfiles.Web.Application.Interfaces.Services.BankProfiles;

public interface IBankDataService
{
   Task<BankProfile?> GetBankByCodeAsync(string bankCode);
   Task<List<BankProfile>> GetAllBanksAsync();
   Task RefreshCacheAsync();
}