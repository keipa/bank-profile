using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class FeesCard
   {
      [JsonPropertyName("cardIssuance")]
      public double? CardIssuance { get; set; }
    
      [JsonPropertyName("premiumCardAnnualFee")]
      public double? PremiumCardAnnualFee { get; set; }
    
      [JsonPropertyName("replacementCardFee")]
      public double? ReplacementCardFee { get; set; }
   }
}