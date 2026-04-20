using BankProfiles.Web.Application.Features.Ratings.Models;

namespace BankProfiles.Web.Application.Interfaces.Services.Ratings;

public interface IChartDataService
{
    Task<ChartDataSet> GetRatingHistoryDataAsync(string bankCode, int days = 30);
    Task<List<ChartDataSet>> GetRatingHistoryByCriteriaAsync(string bankCode, int days = 30);
    Task<ChartDataSet> GetViewHistoryDataAsync(string bankCode, int days = 30);
    Task<List<ChartDataSet>> GetAllBanksRatingComparisonAsync(int criteriaId, int days = 30);
}
