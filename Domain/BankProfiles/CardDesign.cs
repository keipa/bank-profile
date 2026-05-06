using System.Text.Json.Serialization;

namespace BankProfiles.Web.Domain.BankProfiles;

public class CardDesign
{
    [JsonPropertyName("imageUrl")]
    public required string ImageUrl { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("isVertical")]
    public bool IsVertical { get; set; }
}
