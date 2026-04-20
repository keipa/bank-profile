using BankProfiles.Web.Domain.Ratings;

namespace BankProfiles.Web.Application.Interfaces.Services.Ratings;

public interface IRatingService
{
   Task<List<RatingData>> GetBankRatingsAsync(string bankCode);
   Task<List<RatingHistoryPoint>> GetRatingHistoryAsync(string bankCode, int criteriaId, int days = 30);
   Task<decimal> GetOverallRatingAsync(string bankCode);
   Task<List<BankRatingsSummary>> GetAllBankRatingsAsync();
   Task AddRatingHistorySnapshotAsync(CancellationToken cancellationToken = default);
}