namespace BankProfiles.Web.Services;

public interface IChartDataService
{
    Task<ChartDataSet> GetRatingHistoryDataAsync(string bankCode, int days = 30);
    Task<ChartDataSet> GetViewHistoryDataAsync(string bankCode, int days = 30);
    Task<List<ChartDataSet>> GetAllBanksRatingComparisonAsync(int criteriaId, int days = 30);
}