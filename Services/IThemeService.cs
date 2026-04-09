namespace BankProfiles.Web.Services;

public interface IThemeService
{
    string GetCurrentTheme();
    Task SetThemeAsync(string theme);
    string GetSystemPreference();
}
