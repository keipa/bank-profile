using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankProducts
   {
      [JsonPropertyName("accounts")]
      public bool? Accounts { get; set; }
    
      [JsonPropertyName("cards")]
      public bool? Cards { get; set; }
    
      [JsonPropertyName("savings")]
      public bool? Savings { get; set; }
    
      [JsonPropertyName("loans")]
      public bool? Loans { get; set; }
    
      [JsonPropertyName("mortgages")]
      public bool? Mortgages { get; set; }
    
      [JsonPropertyName("investmentTools")]
      public bool? InvestmentTools { get; set; }
    
      [JsonPropertyName("merchantAcquiring")]
      public bool? MerchantAcquiring { get; set; }
    
      [JsonPropertyName("payroll")]
      public bool? Payroll { get; set; }
    
      [JsonPropertyName("escrow")]
      public bool? Escrow { get; set; }
    
      [JsonPropertyName("tradeFinance")]
      public bool? TradeFinance { get; set; }
   }
}