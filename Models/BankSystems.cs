using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankSystems
   {
      [JsonPropertyName("cardSystems")]
      public required List<string> CardSystems { get; set; } // visa, mastercard, amex, unionpay, jcb, diners_club, discover, maestro, mir, other
    
      [JsonPropertyName("swiftAvailable")]
      public required bool SwiftAvailable { get; set; }
    
      [JsonPropertyName("ibanSupported")]
      public required bool IbanSupported { get; set; }
    
      [JsonPropertyName("sepaAvailable")]
      public required bool SepaAvailable { get; set; }
    
      [JsonPropertyName("localClearing")]
      public bool? LocalClearing { get; set; }
    
      [JsonPropertyName("instantTransfers")]
      public bool? InstantTransfers { get; set; }
    
      [JsonPropertyName("cryptoExposure")]
      public string? CryptoExposure { get; set; } // none, low, moderate, high, unknown
   }
}