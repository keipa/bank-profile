using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services;

public class CountryService : ICountryService
{
    private static readonly Dictionary<string, CountryInfo> _countries = new()
    {
        ["uk"] = new() 
        { 
            Code = "uk", 
            Name = "United Kingdom", 
            Flag = "🇬🇧", 
            Currency = "GBP", 
            Language = "en-GB" 
        },
        ["us"] = new() 
        { 
            Code = "us", 
            Name = "United States", 
            Flag = "🇺🇸", 
            Currency = "USD", 
            Language = "en-US" 
        },
        ["de"] = new() 
        { 
            Code = "de", 
            Name = "Germany", 
            Flag = "🇩🇪", 
            Currency = "EUR", 
            Language = "de-DE" 
        },
        ["fr"] = new() 
        { 
            Code = "fr", 
            Name = "France", 
            Flag = "🇫🇷", 
            Currency = "EUR", 
            Language = "fr-FR" 
        },
        ["es"] = new() 
        { 
            Code = "es", 
            Name = "Spain", 
            Flag = "🇪🇸", 
            Currency = "EUR", 
            Language = "es-ES" 
        }
    };

    public CountryInfo? GetCountryInfo(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            return null;

        var code = countryCode.ToLowerInvariant();
        return _countries.TryGetValue(code, out var country) ? country : null;
    }

    public IEnumerable<CountryInfo> GetAllCountries()
    {
        return _countries.Values;
    }
}
