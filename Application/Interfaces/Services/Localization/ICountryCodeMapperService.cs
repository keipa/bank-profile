namespace BankProfiles.Web.Application.Interfaces.Services.Localization;

public interface ICountryCodeMapperService
{
    bool TryGetIso2Code(string countryNameOrCode, out string iso2Code);
}