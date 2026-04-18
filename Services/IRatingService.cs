using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services
{
   public interface IRatingService
   {
      Task<List<RatingData>> GetBankRatingsAsync(string bankCode);
      Task<List<RatingHistoryPoint>> GetRatingHistoryAsync(string bankCode, int criteriaId, int days = 30);
      Task<decimal> GetOverallRatingAsync(string bankCode);
      Task<List<BankRatingsSummary>> GetAllBankRatingsAsync();
      Task AddRatingHistorySnapshotAsync();
   }
}