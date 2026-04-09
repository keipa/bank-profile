namespace BankProfiles.Web.Services;

public interface IChartDataService
{
    Task<ChartDataSet> GetRatingHistoryDataAsync(string bankCode, int days = 30);
    Task<ChartDataSet> GetViewHistoryDataAsync(string bankCode, int days = 30);
    Task<List<ChartDataSet>> GetAllBanksRatingComparisonAsync(int criteriaId, int days = 30);
}

public class ChartDataSet
{
    public string Label { get; set; } = string.Empty;
    public List<string> Labels { get; set; } = new();
    public List<decimal?> Data { get; set; } = new();
    public string BorderColor { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
}
