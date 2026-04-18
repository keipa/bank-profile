using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankRatings
   {
      [JsonPropertyName("overall")]
      public required double Overall { get; set; } // 0-5
    
      [JsonPropertyName("history")]
      public List<RatingPoint>? History { get; set; }
    
      [JsonPropertyName("source")]
      public string? Source { get; set; }
    
      [JsonPropertyName("lastUpdated")]
      public DateTime? LastUpdated { get; set; }
   }
}