using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services
{
   public interface ICountryService
   {
      CountryInfo? GetCountryInfo(string countryCode);
      IEnumerable<CountryInfo> GetAllCountries();
   }
}