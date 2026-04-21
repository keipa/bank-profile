using BankProfiles.Web.Application.Interfaces.Services.Localization;

namespace BankProfiles.Web.Application.Features.Localization.Services;

public class ThemeService : IThemeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ThemeService> _logger;
    private const string ThemeCookieName = "theme";
    private const string DefaultTheme = "light";
    private readonly HashSet<string> _validThemes = new() { "light", "dark" };

    public event Action<string>? OnThemeChanged;

    public ThemeService(IHttpContextAccessor httpContextAccessor, ILogger<ThemeService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string GetCurrentTheme()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return DefaultTheme;
        }

        if (context.Request.Cookies.TryGetValue(ThemeCookieName, out var theme) &&
            _validThemes.Contains(theme))
        {
            return theme;
        }

        return DefaultTheme;
    }

    public Task SetThemeAsync(string theme)
    {
        if (!_validThemes.Contains(theme))
        {
            throw new ArgumentException($"Invalid theme value. Must be one of: {string.Join(", ", _validThemes)}", nameof(theme));
        }

        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new InvalidOperationException("HTTP context is not available.");
        }

        var cookieOptions = new CookieOptions
        {
            Path = "/",
            MaxAge = TimeSpan.FromDays(365),
            HttpOnly = false, // JavaScript needs to read this
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax
        };

        try
        {
            if (!context.Response.HasStarted)
            {
                context.Response.Cookies.Append(ThemeCookieName, theme, cookieOptions);
            }
        }
        catch (InvalidOperationException)
        {
            // Headers already sent - cookie will be set via JavaScript in ThemeToggle.razor
        }

        OnThemeChanged?.Invoke(theme);

        return Task.CompletedTask;
    }

    public string GetSystemPreference() =>
       // Could be enhanced to detect system preference via user agent or other means
       // For now, return default
       DefaultTheme;
}
