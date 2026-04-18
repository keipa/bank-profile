using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models;

public class BankProfile
{
    // Core required fields
    [JsonPropertyName("bankId")]
    public required string BankId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("legalName")]
    public required string LegalName { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; } // active, inactive, restricted, under_review, sanctioned, closed

    [JsonPropertyName("countryOfOwnerResidence")]
    public required string CountryOfOwnerResidence { get; set; }

    [JsonPropertyName("headquartersCountry")]
    public required string HeadquartersCountry { get; set; }

    [JsonPropertyName("jurisdiction")]
    public string? Jurisdiction { get; set; }

    // Nested required objects
    [JsonPropertyName("overview")]
    public BankOverview? Overview { get; set; }

    [JsonPropertyName("systems")]
    public required BankSystems Systems { get; set; }

    [JsonPropertyName("currencies")]
    public required BankCurrencies Currencies { get; set; }

    [JsonPropertyName("fees")]
    public required BankFees Fees { get; set; }

    [JsonPropertyName("branches")]
    public required BankBranches Branches { get; set; }

    [JsonPropertyName("clients")]
    public required BankClients Clients { get; set; }

    [JsonPropertyName("ratings")]
    public required BankRatings Ratings { get; set; }

    [JsonPropertyName("compliance")]
    public required BankCompliance Compliance { get; set; }

    [JsonPropertyName("digitalChannels")]
    public required DigitalChannels DigitalChannels { get; set; }

    // Optional objects
    [JsonPropertyName("support")]
    public BankSupport? Support { get; set; }

    [JsonPropertyName("products")]
    public BankProducts? Products { get; set; }

    [JsonPropertyName("transactions")]
    public BankTransactions? Transactions { get; set; }

    [JsonPropertyName("redFlags")]
    public List<RedFlagEvent>? RedFlags { get; set; }

    [JsonPropertyName("metrics")]
    public BankMetrics? Metrics { get; set; }

    [JsonPropertyName("metadata")]
    public BankMetadata? Metadata { get; set; }

    // Custom extensions (not in schema but needed for UI)
    [JsonPropertyName("theme")]
    public BankTheme? Theme { get; set; }

    [JsonPropertyName("animationConfig")]
    public AnimationConfig? AnimationConfig { get; set; }


    [JsonPropertyName("defaultLanguage")]
    public string? DefaultLanguage { get; set; }

    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [JsonPropertyName("contactInfo")]
    public ContactInfo? ContactInfo { get; set; }

    // Backward compatibility properties
    [JsonIgnore]
    public string BankCode => BankId;

    [JsonIgnore]
    public string? CountryCode => HeadquartersCountry?.ToLowerInvariant() switch
    {
        "united states" => "us",
        "united kingdom" => "uk",
        "germany" => "de",
        "france" => "fr",
        "spain" => "es",
        "russia" => "ru",
        "georgia" => "ge",
        _ => null
    };

    [JsonIgnore]
    public string BankName => Name;

    [JsonIgnore]
    public string? Description => Overview?.Description;

    [JsonIgnore]
    public string? LogoUrl => Overview?.LogoUrl;

    [JsonIgnore]
    public string? IconUrl => Overview?.IconUrl;

    [JsonIgnore]
    public TechnicalInfo? TechnicalInfo => null; // Deprecated, data now in systems/fees/currencies
}
