using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class FeesAccount
   {
      [JsonPropertyName("accountOpening")]
      public double? AccountOpening { get; set; }
    
      [JsonPropertyName("monthlyMaintenance")]
      public double? MonthlyMaintenance { get; set; }
    
      [JsonPropertyName("accountClosure")]
      public double? AccountClosure { get; set; }
    
      [JsonPropertyName("dormancyAfterMonths")]
      public int? DormancyAfterMonths { get; set; }
    
      [JsonPropertyName("dormancyFee")]
      public double? DormancyFee { get; set; }
   }
}