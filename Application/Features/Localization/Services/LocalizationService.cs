using System.Text.Json;
using BankProfiles.Web.Application.Interfaces.Services.Localization;

namespace BankProfiles.Web.Application.Features.Localization.Services;

public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalizationService> _logger;
    private readonly Dictionary<string, Dictionary<string, string>> _resourceCache;
    private const string CookieName = "lang";
    private const string DefaultLanguage = "en-US";
    private static readonly string[] SupportedLanguages = { "en-US", "en-GB", "de-DE", "fr-FR", "es-ES", "ru-RU", "ka-GE" };

    private static readonly Dictionary<string, string> CountryToLanguageMap = new()
    {
        { "uk", "en-GB" },
        { "gb", "en-GB" },
        { "us", "en-US" },
        { "de", "de-DE" },
        { "fr", "fr-FR" },
        { "es", "es-ES" },
        { "ru", "ru-RU" },
        { "ge", "ka-GE" }
    };

    public LocalizationService(
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment environment,
        ILogger<LocalizationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
        _logger = logger;
        _resourceCache = new Dictionary<string, Dictionary<string, string>>();
        LoadAllResources();
    }

    /// <summary>
    /// Retrieves the current language based on the user's cookie preference.
    /// Falls back to default language (en-US) if cookie is not set or invalid.
    /// </summary>
    public string GetCurrentLanguage()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Cookies.TryGetValue(CookieName, out var language) == true)
        {
            // Validate that the language from cookie is supported
            if (SupportedLanguages.Contains(language))
            {
                return language;
            }
        }
        return DefaultLanguage;
    }

    /// <summary>
    /// Sets the user's language preference via cookie.
    /// Cookie persists for 365 days and is accessible across all pages.
    /// </summary>
    public Task SetLanguageAsync(string languageCode)
    {
        // Validate language code against supported languages list
        if (!SupportedLanguages.Contains(languageCode))
        {
            throw new ArgumentException($"Unsupported language: {languageCode}", nameof(languageCode));
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            try
            {
                var cookieOptions = new CookieOptions
                {
                    Path = "/",
                    MaxAge = TimeSpan.FromDays(365),  // Persist for 1 year
                    HttpOnly = false,                 // Accessible via JavaScript
                    IsEssential = true,               // GDPR compliance - essential functionality
                    SameSite = SameSiteMode.Lax       // CSRF protection
                };
                httpContext.Response.Cookies.Append(CookieName, languageCode, cookieOptions);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Unable to set language cookie to {LanguageCode}. Path: {Path}",
                    languageCode,
                    httpContext.Request.Path);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a localized string for the given key.
    /// Falls back to default language (en-US) if key is not found in current language.
    /// Returns the key itself if not found in any language (fail-safe approach).
    /// </summary>
    public string GetString(string key)
    {
        var language = GetCurrentLanguage();
        
        // Try to get string from current language resources
        if (_resourceCache.TryGetValue(language, out var resources))
        {
            if (resources.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        // Fallback: Try default language if current language doesn't have the key
        // This prevents missing translations from breaking the UI
        if (language != DefaultLanguage && _resourceCache.TryGetValue(DefaultLanguage, out var defaultResources))
        {
            if (defaultResources.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        // Ultimate fallback: return the key itself as a visual indicator
        // This makes it obvious that a translation is missing during development
        return key;
    }

    /// <summary>
    /// Maps a country code to its default language.
    /// Used for automatic language detection based on bank's country.
    /// </summary>
    public string GetDefaultLanguageForCountry(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return DefaultLanguage;
        }

        var normalizedCode = countryCode.ToLowerInvariant();
        return CountryToLanguageMap.TryGetValue(normalizedCode, out var language) 
            ? language 
            : DefaultLanguage;
    }

    private void LoadAllResources()
    {
        var resourcesPath = Path.Combine(_environment.ContentRootPath, "Resources");
        
        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }

        foreach (var language in SupportedLanguages)
        {
            var filePath = Path.Combine(resourcesPath, $"Strings.{language}.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var resources = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (resources != null)
                    {
                        _resourceCache[language] = resources;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to deserialize localization resources for language {Language} from {FilePath}",
                        language,
                        filePath);
                    _resourceCache[language] = new Dictionary<string, string>();
                }
                catch (IOException ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to read localization resources for language {Language} from {FilePath}",
                        language,
                        filePath);
                    _resourceCache[language] = new Dictionary<string, string>();
                }
            }
            else
            {
                _resourceCache[language] = new Dictionary<string, string>();
            }
        }
    }
}
