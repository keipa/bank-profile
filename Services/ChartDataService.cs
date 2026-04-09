using BankProfiles.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Web.Services;

public class ChartDataService : IChartDataService
{
    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<ChartDataService> _logger;

    public ChartDataService(IDbContextFactory<BankDbContext> contextFactory, ILogger<ChartDataService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<ChartDataSet> GetRatingHistoryDataAsync(string bankCode, int days = 30)
    {
        // Validate bank code format to prevent SQL injection
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return new ChartDataSet { Label = bankCode };
        }

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Calculate the start date for the time range (e.g., 30 days ago)
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            
            var bank = await context.Banks
                .FirstOrDefaultAsync(b => b.BankCode == bankCode);

            if (bank == null)
            {
                _logger.LogWarning("Bank with code {BankCode} not found", bankCode);
                return new ChartDataSet { Label = bankCode };
            }

            // Query rating history and group by date to get daily averages
            // This aggregates all criteria ratings into a single overall rating per day
            var ratingData = await context.RatingHistories
                .Where(rh => rh.BankId == bank.BankId && rh.RecordedDate >= startDate)
                .GroupBy(rh => rh.RecordedDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    AverageRating = g.Average(rh => rh.OverallRating)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Create a complete date range to fill gaps in the data
            // This ensures the chart shows all dates even if no ratings exist
            var dateRange = Enumerable.Range(0, days + 1)
                .Select(offset => startDate.AddDays(offset).Date)
                .ToList();

            var labels = new List<string>();
            var data = new List<decimal?>();
            decimal? lastKnownValue = null;

            // Populate chart data, filling gaps with the last known value
            // This creates a continuous line rather than broken segments
            foreach (var date in dateRange)
            {
                labels.Add(date.ToString("MM/dd"));
                
                var dataPoint = ratingData.FirstOrDefault(d => d.Date == date);
                if (dataPoint != null)
                {
                    lastKnownValue = dataPoint.AverageRating;
                    data.Add(dataPoint.AverageRating);
                }
                else
                {
                    // Use last known value to fill gaps (forward-fill strategy)
                    data.Add(lastKnownValue);
                }
            }

            var color = GetColorForBank(bankCode);

            return new ChartDataSet
            {
                Label = $"{bankCode} Rating",
                Labels = labels,
                Data = data,
                BorderColor = color,
                BackgroundColor = AddAlpha(color, 0.2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rating history for bank {BankCode}", bankCode);
            return new ChartDataSet { Label = bankCode };
        }
    }

    public async Task<ChartDataSet> GetViewHistoryDataAsync(string bankCode, int days = 30)
    {
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return new ChartDataSet { Label = bankCode };
        }

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            
            var bank = await context.Banks
                .FirstOrDefaultAsync(b => b.BankCode == bankCode);

            if (bank == null)
            {
                _logger.LogWarning("Bank with code {BankCode} not found", bankCode);
                return new ChartDataSet { Label = bankCode };
            }

            // Get view history grouped by date
            var viewData = await context.ViewHistory
                .Where(vh => vh.BankId == bank.BankId && vh.RecordedDate >= startDate)
                .GroupBy(vh => vh.RecordedDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalViews = g.Sum(vh => vh.ViewCount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Fill gaps in data with all dates in range
            var dateRange = Enumerable.Range(0, days + 1)
                .Select(offset => startDate.AddDays(offset).Date)
                .ToList();

            var labels = new List<string>();
            var data = new List<decimal?>();

            foreach (var date in dateRange)
            {
                labels.Add(date.ToString("MM/dd"));
                
                var dataPoint = viewData.FirstOrDefault(d => d.Date == date);
                if (dataPoint != null)
                {
                    data.Add(dataPoint.TotalViews);
                }
                else
                {
                    // Fill gaps with zero for view counts
                    data.Add(0);
                }
            }

            var color = GetColorForBank(bankCode);

            return new ChartDataSet
            {
                Label = $"{bankCode} Views",
                Labels = labels,
                Data = data,
                BorderColor = color,
                BackgroundColor = AddAlpha(color, 0.2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting view history for bank {BankCode}", bankCode);
            return new ChartDataSet { Label = bankCode };
        }
    }

    public async Task<List<ChartDataSet>> GetAllBanksRatingComparisonAsync(int criteriaId, int days = 30)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            
            // Get all banks
            var banks = await context.Banks.ToListAsync();

            var dataSets = new List<ChartDataSet>();

            foreach (var bank in banks)
            {
                // Get rating history for specific criteria
                var ratingData = await context.RatingHistories
                    .Where(rh => rh.BankId == bank.BankId 
                              && rh.CriteriaId == criteriaId 
                              && rh.RecordedDate >= startDate)
                    .GroupBy(rh => rh.RecordedDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        AverageRating = g.Average(rh => rh.OverallRating)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                // Fill gaps in data with all dates in range
                var dateRange = Enumerable.Range(0, days + 1)
                    .Select(offset => startDate.AddDays(offset).Date)
                    .ToList();

                var labels = new List<string>();
                var data = new List<decimal?>();
                decimal? lastKnownValue = null;

                foreach (var date in dateRange)
                {
                    if (dataSets.Count == 0) // Only add labels once
                    {
                        labels.Add(date.ToString("MM/dd"));
                    }
                    
                    var dataPoint = ratingData.FirstOrDefault(d => d.Date == date);
                    if (dataPoint != null)
                    {
                        lastKnownValue = dataPoint.AverageRating;
                        data.Add(dataPoint.AverageRating);
                    }
                    else
                    {
                        // Use last known value to fill gaps
                        data.Add(lastKnownValue);
                    }
                }

                var color = GetColorForBank(bank.BankCode);

                dataSets.Add(new ChartDataSet
                {
                    Label = bank.BankCode,
                    Labels = dataSets.Count == 0 ? labels : dataSets[0].Labels,
                    Data = data,
                    BorderColor = color,
                    BackgroundColor = AddAlpha(color, 0.2)
                });
            }

            return dataSets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rating comparison for criteria {CriteriaId}", criteriaId);
            return new List<ChartDataSet>();
        }
    }

    private string GetColorForBank(string bankCode)
    {
        // Generate consistent colors based on bank code hash
        var hash = Math.Abs(bankCode.GetHashCode());
        var hue = hash % 360;
        
        // Use HSL to generate pleasant colors with good saturation and lightness
        return HslToRgb(hue, 70, 50);
    }

    private string GetColorForCriteria(int criteriaId)
    {
        // Predefined color palette for criteria
        var colors = new[]
        {
            "rgb(255, 99, 132)",   // Red
            "rgb(54, 162, 235)",   // Blue
            "rgb(255, 206, 86)",   // Yellow
            "rgb(75, 192, 192)",   // Teal
            "rgb(153, 102, 255)",  // Purple
            "rgb(255, 159, 64)",   // Orange
            "rgb(46, 204, 113)",   // Green
            "rgb(241, 196, 15)"    // Gold
        };

        return colors[(criteriaId - 1) % colors.Length];
    }

    private string HslToRgb(int hue, int saturation, int lightness)
    {
        double h = hue / 360.0;
        double s = saturation / 100.0;
        double l = lightness / 100.0;

        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double HueToRgb(double p, double q, double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1.0 / 6) return p + (q - p) * 6 * t;
                if (t < 1.0 / 2) return q;
                if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
                return p;
            }

            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;

            r = HueToRgb(p, q, h + 1.0 / 3);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3);
        }

        return $"rgb({(int)(r * 255)}, {(int)(g * 255)}, {(int)(b * 255)})";
    }

    private string AddAlpha(string rgbColor, double alpha)
    {
        // Convert rgb(r, g, b) to rgba(r, g, b, alpha)
        if (rgbColor.StartsWith("rgb(") && rgbColor.EndsWith(")"))
        {
            var values = rgbColor.Substring(4, rgbColor.Length - 5);
            return $"rgba({values}, {alpha})";
        }

        return rgbColor;
    }
}
