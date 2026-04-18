using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class ThemeColors
   {
      [JsonPropertyName("primaryColor")]
      public required string PrimaryColor { get; set; }
    
      [JsonPropertyName("secondaryColor")]
      public required string SecondaryColor { get; set; }
    
      [JsonPropertyName("accentColor")]
      public required string AccentColor { get; set; }
    
      [JsonPropertyName("backgroundStart")]
      public required string BackgroundStart { get; set; }
    
      [JsonPropertyName("backgroundEnd")]
      public required string BackgroundEnd { get; set; }
   }
}