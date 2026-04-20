using BankProfiles.Web.Domain.Common.Metrics;

namespace BankProfiles.Web.Presentation.Services.Modal;

public class ModalService
{
    private MetricDto? _selectedMetric;
    private bool _isVisible;

    public MetricDto? SelectedMetric => _selectedMetric;
    public bool IsVisible => _isVisible;

    public event Action? OnModalStateChanged;

    public void ShowModal(MetricDto metric)
    {
        _selectedMetric = metric;
        _isVisible = true;
        NotifyStateChanged();
    }

    public void HideModal()
    {
        _isVisible = false;
        _selectedMetric = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnModalStateChanged?.Invoke();
}
