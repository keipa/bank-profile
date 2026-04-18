namespace BankProfiles.Web.Services;

public interface ICountryCodeMapperService
{
    bool TryGetIso2Code(string countryNameOrCode, out string iso2Code);
}