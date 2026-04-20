namespace BankProfiles.Web.Application.Interfaces.Services.Ratings;

public interface IViewCountService
{
   Task IncrementViewCountAsync(string bankCode);
   Task<long> GetViewCountAsync(string bankCode);
   Task<List<(string BankCode, long ViewCount)>> GetMostViewedBanksAsync(int topN = 10);
   Task RecordViewHistorySnapshotAsync(string bankCode);
}