using System.Text.Json.Serialization;

namespace BankProfiles.Web.Domain.BankProfiles;

public class TransactionDestination
{
   [JsonPropertyName("country")]
   public required string Country { get; set; }
}