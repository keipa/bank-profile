namespace BankProfiles.Web.Application.Interfaces.Services.Localization;

public interface IThemeService
{
    string GetCurrentTheme();
    Task SetThemeAsync(string theme);
    string GetSystemPreference();
    event Action<string>? OnThemeChanged;
}
