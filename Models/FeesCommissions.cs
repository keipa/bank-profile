using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class FeesCommissions
   {
      [JsonPropertyName("incomingDomesticPercent")]
      public double? IncomingDomesticPercent { get; set; }
    
      [JsonPropertyName("incomingInternationalPercent")]
      public double? IncomingInternationalPercent { get; set; }
    
      [JsonPropertyName("outgoingDomesticPercent")]
      public double? OutgoingDomesticPercent { get; set; }
    
      [JsonPropertyName("outgoingInternationalPercent")]
      public double? OutgoingInternationalPercent { get; set; }
    
      [JsonPropertyName("cashWithdrawalLocalAtmPercent")]
      public double? CashWithdrawalLocalAtmPercent { get; set; }
    
      [JsonPropertyName("cashWithdrawalInternationalAtmPercent")]
      public double? CashWithdrawalInternationalAtmPercent { get; set; }
    
      [JsonPropertyName("fxMarkupPercent")]
      public double? FxMarkupPercent { get; set; }
   }
}