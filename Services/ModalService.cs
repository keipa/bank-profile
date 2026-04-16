using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services;

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

    private void NotifyStateChanged()
    {
        OnModalStateChanged?.Invoke();
    }
}
