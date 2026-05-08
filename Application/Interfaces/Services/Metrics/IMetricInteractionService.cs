using BankProfiles.Web.Domain.Common.Metrics;

namespace BankProfiles.Web.Application.Interfaces.Services.Metrics;

/// <summary>
/// Service for handling user interactions with metric tiles (clicks, modal display).
/// </summary>
public interface IMetricInteractionService
{
    /// <summary>
    /// Handles a metric tile click by opening the metric detail modal.
    /// </summary>
    /// <param name="metric">The metric that was clicked</param>
    /// <param name="bankCode">The bank code for context</param>
    /// <param name="accentColor">The bank's accent color for modal styling</param>
    void HandleMetricClick(MetricDto metric, string bankCode, string accentColor);
}
