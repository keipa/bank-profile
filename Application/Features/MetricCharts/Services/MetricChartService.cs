using System.Globalization;
using BankProfiles.Web.Application.Features.MetricCharts;
using BankProfiles.Web.Application.Features.MetricCharts.Models;
using BankProfiles.Web.Application.Interfaces.Repositories.EventSourcing;
using BankProfiles.Web.Application.Interfaces.Services.BankProfiles;
using BankProfiles.Web.Application.Interfaces.Services.MetricCharts;

namespace BankProfiles.Web.Application.Features.MetricCharts.Services;

public class MetricChartService : IMetricChartService
{
    private readonly IEventStoreService _eventStoreService;
    private readonly IBankDataService _bankDataService;
    private readonly ILogger<MetricChartService> _logger;

    public MetricChartService(
        IEventStoreService eventStoreService,
        IBankDataService bankDataService,
        ILogger<MetricChartService> logger)
    {
        _eventStoreService = eventStoreService;
        _bankDataService = bankDataService;
        _logger = logger;
    }

    public bool IsChartableMetric(string metricKey) =>
        MetricChartMappings.KeyToEventName.ContainsKey(metricKey);

    public async Task<MetricHistoryData?> GetMetricHistoryAsync(string bankCode, string metricKey)
    {
        if (!MetricChartMappings.KeyToEventName.TryGetValue(metricKey, out var eventMetricName))
            return null;

        try
        {
            var events = await _eventStoreService.GetEventsByMetricAsync(bankCode, eventMetricName);

            var points = new List<MetricHistoryPoint>();
            foreach (var evt in events)
            {
                if (TryParseNumeric(evt.MetricValue, out var value))
                {
                    points.Add(new MetricHistoryPoint
                    {
                        Date = evt.CreatedDate,
                        Value = value
                    });
                }
            }

            return new MetricHistoryData
            {
                MetricName = eventMetricName,
                Points = points
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get metric history for {BankCode}/{MetricKey}", bankCode, metricKey);
            return null;
        }
    }

    public async Task<MetricComparisonData?> GetMetricComparisonAsync(string bankCode, string metricKey)
    {
        if (!MetricChartMappings.KeyToEventName.TryGetValue(metricKey, out var eventMetricName))
            return null;

        try
        {
            var latestEvents = await _eventStoreService.GetLatestEventByMetricAcrossBanksAsync(eventMetricName);
            var allBanks = await _bankDataService.GetAllBanksAsync();
            var bankNameMap = allBanks.ToDictionary(b => b.BankCode, b => b.Name, StringComparer.OrdinalIgnoreCase);

            var items = new List<MetricComparisonItem>();
            foreach (var evt in latestEvents)
            {
                if (TryParseNumeric(evt.MetricValue, out var value))
                {
                    items.Add(new MetricComparisonItem
                    {
                        BankCode = evt.BankCode,
                        BankName = bankNameMap.GetValueOrDefault(evt.BankCode, evt.BankCode),
                        Value = value,
                        IsCurrentBank = string.Equals(evt.BankCode, bankCode, StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            // Sort descending by value; current bank stays in natural position
            items.Sort((a, b) => b.Value.CompareTo(a.Value));

            return new MetricComparisonData
            {
                MetricName = eventMetricName,
                Items = items
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get metric comparison for {BankCode}/{MetricKey}", bankCode, metricKey);
            return null;
        }
    }

    private static bool TryParseNumeric(string metricValue, out double result)
    {
        // Event values are JSON-serialized; strip surrounding quotes if present
        var cleaned = metricValue.Trim().Trim('"');
        return double.TryParse(cleaned, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
    }
}
