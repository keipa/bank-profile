using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankMetadata
   {
      [JsonPropertyName("createdAt")]
      public DateTime? CreatedAt { get; set; }
    
      [JsonPropertyName("updatedAt")]
      public DateTime? UpdatedAt { get; set; }
    
      [JsonPropertyName("lastReviewed")]
      public DateTime? LastReviewed { get; set; }
    
      [JsonPropertyName("source")]
      public string? Source { get; set; }
    
      [JsonPropertyName("confidence")]
      public double? Confidence { get; set; } // 0-1
   }
}