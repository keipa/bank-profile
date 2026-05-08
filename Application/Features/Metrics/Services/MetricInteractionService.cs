using BankProfiles.Web.Application.Interfaces.Services.Metrics;
using BankProfiles.Web.Domain.Common.Metrics;
using BankProfiles.Web.Presentation.Services.Modal;

namespace BankProfiles.Web.Application.Features.Metrics.Services;

/// <summary>
/// Service for handling user interactions with metric tiles.
/// </summary>
public class MetricInteractionService : IMetricInteractionService
{
    private readonly ModalService _modalService;

    public MetricInteractionService(ModalService modalService)
    {
        _modalService = modalService;
    }

    /// <inheritdoc />
    public void HandleMetricClick(MetricDto metric, string bankCode, string accentColor)
    {
        _modalService.ShowModal(metric, bankCode, accentColor);
    }
}
