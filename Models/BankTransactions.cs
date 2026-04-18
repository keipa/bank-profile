using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankTransactions
   {
      [JsonPropertyName("outgoingDestinations")]
      public List<TransactionDestination>? OutgoingDestinations { get; set; }
   }
}