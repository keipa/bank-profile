namespace BankProfiles.Web.Services;

public class ThemeService : IThemeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string ThemeCookieName = "theme";
    private const string DefaultTheme = "light";
    private readonly HashSet<string> _validThemes = new() { "light", "dark" };

    public event Action<string>? OnThemeChanged;

    public ThemeService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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
            context.Response.Cookies.Append(ThemeCookieName, theme, cookieOptions);
            OnThemeChanged?.Invoke(theme);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Headers are read-only"))
        {
            // Headers already sent - this is expected when called from event handlers
            // Cookie should be set via JavaScript instead (see ThemeToggle.razor)
            // Just log and continue - theme will be applied via JS
            Console.WriteLine($"Warning: Cannot set theme cookie server-side (headers already sent). Use JavaScript cookie setting instead.");
        }

        return Task.CompletedTask;
    }

    public string GetSystemPreference()
    {
        // Could be enhanced to detect system preference via user agent or other means
        // For now, return default
        return DefaultTheme;
    }
}
