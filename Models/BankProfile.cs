namespace BankProfiles.Web.Models;

public class BankProfile
{
    public required string BankCode { get; set; }
    public string? CountryCode { get; set; }
    public string? DefaultLanguage { get; set; }
    public required string BankName { get; set; }
    public string? Description { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public BankTheme? Theme { get; set; }
    public TechnicalInfo? TechnicalInfo { get; set; }
    public ContactInfo? ContactInfo { get; set; }
    public AnimationConfig? AnimationConfig { get; set; }
}
