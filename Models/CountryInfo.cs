namespace BankProfiles.Web.Models;

public class CountryInfo
{
    public required string Code { get; set; }  // "uk", "us", etc.
    public required string Name { get; set; }  // "United Kingdom"
    public required string Flag { get; set; }  // "🇬🇧"
    public required string Currency { get; set; }  // "GBP"
    public required string Language { get; set; }  // "en-GB"
}
