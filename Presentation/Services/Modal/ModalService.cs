using BankProfiles.Web.Domain.Common.Metrics;

namespace BankProfiles.Web.Presentation.Services.Modal;

public class ModalService
{
    private MetricDto? _selectedMetric;
    private bool _isVisible;

    public MetricDto? SelectedMetric => _selectedMetric;
    public bool IsVisible => _isVisible;
    public string? CurrentBankCode { get; private set; }
    public string? AccentColor { get; private set; }

    public event Action? OnModalStateChanged;

    public void ShowModal(MetricDto metric, string bankCode, string? accentColor = null)
    {
        _selectedMetric = metric;
        CurrentBankCode = bankCode;
        AccentColor = accentColor;
        _isVisible = true;
        NotifyStateChanged();
    }

    public void HideModal()
    {
        _isVisible = false;
        _selectedMetric = null;
        CurrentBankCode = null;
        AccentColor = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnModalStateChanged?.Invoke();
}
