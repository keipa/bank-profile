namespace BankProfiles.Web.Services;

public interface ILocalizationService
{
    string GetCurrentLanguage();
    Task SetLanguageAsync(string languageCode);
    string GetString(string key);
    string GetDefaultLanguageForCountry(string countryCode);
}
