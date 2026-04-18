using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankCurrencies
   {
      [JsonPropertyName("available")]
      public required List<string> Available { get; set; } // ISO 4217 currency codes
    
      [JsonPropertyName("baseCurrency")]
      public string? BaseCurrency { get; set; }
    
      [JsonPropertyName("multiCurrencyAccounts")]
      public bool? MultiCurrencyAccounts { get; set; }
    
      [JsonPropertyName("fxMarkupPercent")]
      public double? FxMarkupPercent { get; set; }
   }
}