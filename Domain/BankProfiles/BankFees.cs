using System.Text.Json.Serialization;

namespace BankProfiles.Web.Domain.BankProfiles;

public class BankFees
{
   [JsonPropertyName("commissions")]
   public required FeesCommissions Commissions { get; set; }
    
   [JsonPropertyName("accountFees")]
   public required FeesAccount AccountFees { get; set; }
    
   [JsonPropertyName("cardFees")]
   public required FeesCard CardFees { get; set; }
    
   [JsonPropertyName("transferFees")]
   public required FeesTransfer TransferFees { get; set; }
}