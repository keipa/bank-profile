using System.Text.Json.Serialization;

namespace BankProfiles.Web.Domain.BankProfiles;

public class FeesTransfer
{
   [JsonPropertyName("swiftPaymentProcessing")]
   public double? SwiftPaymentProcessing { get; set; }
    
   [JsonPropertyName("urgentPaymentSurcharge")]
   public double? UrgentPaymentSurcharge { get; set; }
    
   [JsonPropertyName("chargebackHandling")]
   public double? ChargebackHandling { get; set; }
}