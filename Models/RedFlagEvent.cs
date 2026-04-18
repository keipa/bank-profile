using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class RedFlagEvent
   {
      [JsonPropertyName("date")]
      public required string Date { get; set; } // date format
    
      [JsonPropertyName("title")]
      public required string Title { get; set; }
    
      [JsonPropertyName("description")]
      public string? Description { get; set; }
    
      [JsonPropertyName("severity")]
      public required string Severity { get; set; } // low, moderate, high, critical
    
      [JsonPropertyName("status")]
      public required string Status { get; set; } // open, monitoring, resolved, closed
    
      [JsonPropertyName("impact")]
      public string? Impact { get; set; } // none, low, moderate, high
   }
}