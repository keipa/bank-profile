using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class RatingPoint
   {
      [JsonPropertyName("date")]
      public required string Date { get; set; } // date format
    
      [JsonPropertyName("rating")]
      public required double Rating { get; set; } // 0-5
    
      [JsonPropertyName("note")]
      public string? Note { get; set; }
   }
}