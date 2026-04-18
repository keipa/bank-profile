using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models;

public class BankOverview
{
    [JsonPropertyName("type")]
    public string? Type { get; set; } // retail bank, digital bank, commercial bank, private bank
    
    [JsonPropertyName("segment")]
    public string? Segment { get; set; }
    
    [JsonPropertyName("foundedYear")]
    public int? FoundedYear { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }
}