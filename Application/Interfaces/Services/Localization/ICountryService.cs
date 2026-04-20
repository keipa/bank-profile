using BankProfiles.Web.Domain.BankProfiles;

namespace BankProfiles.Web.Application.Interfaces.Services.Localization;

public interface ICountryService
{
   CountryInfo? GetCountryInfo(string countryCode);
   IEnumerable<CountryInfo> GetAllCountries();
}