using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class TransactionDestination
   {
      [JsonPropertyName("country")]
      public required string Country { get; set; }
   }
}