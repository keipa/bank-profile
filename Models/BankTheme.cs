using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models;

public class BankTheme
{
    // Backward compatibility - single theme colors (deprecated but maintained)
    [JsonPropertyName("primaryColor")]
    public string PrimaryColor { get; set; } = "#1a237e";
    
    [JsonPropertyName("secondaryColor")]
    public string SecondaryColor { get; set; } = "#3949ab";
    
    [JsonPropertyName("fontFamily")]
    public string FontFamily { get; set; } = "Roboto, sans-serif";
    
    [JsonPropertyName("accentColor")]
    public string AccentColor { get; set; } = "#ff6f00";
    
    // NEW: Dual theme support for dark/light modes
    [JsonPropertyName("darkTheme")]
    public ThemeColors? DarkTheme { get; set; }
    
    [JsonPropertyName("lightTheme")]
    public ThemeColors? LightTheme { get; set; }
}