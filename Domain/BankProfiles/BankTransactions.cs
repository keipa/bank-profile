using System.Text.Json.Serialization;

namespace BankProfiles.Web.Domain.BankProfiles;

public class BankTransactions
{
   [JsonPropertyName("outgoingDestinations")]
   public List<TransactionDestination>? OutgoingDestinations { get; set; }
}