using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankSupport
   {
      [JsonPropertyName("available24x7")]
      public bool? Available24x7 { get; set; }
    
      [JsonPropertyName("channels")]
      public List<string>? Channels { get; set; } // phone, email, chat, branch, app, web
    
      [JsonPropertyName("averageResponseTimeMinutes")]
      public double? AverageResponseTimeMinutes { get; set; }
    
      [JsonPropertyName("languages")]
      public List<string>? Languages { get; set; }
   }
}