using System.Text.Json.Serialization;

namespace BankProfiles.Web.Domain.BankProfiles;

public class BankTheme
{
    [JsonPropertyName("accentColor")]
    public string AccentColor { get; set; } = "#007AFF";
}