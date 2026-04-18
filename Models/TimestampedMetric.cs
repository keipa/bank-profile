using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class TimestampedMetric
   {
      [JsonPropertyName("date")]
      public required string Date { get; set; } // date format
    
      [JsonPropertyName("value")]
      public required double Value { get; set; }
   }
}