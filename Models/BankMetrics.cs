using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankMetrics
   {
      [JsonPropertyName("clientSatisfactionPercent")]
      public double? ClientSatisfactionPercent { get; set; }
    
      [JsonPropertyName("corporateSatisfactionPercent")]
      public double? CorporateSatisfactionPercent { get; set; }
    
      [JsonPropertyName("complaintRatioPercent")]
      public double? ComplaintRatioPercent { get; set; }
    
      [JsonPropertyName("avgRemediationDays")]
      public double? AvgRemediationDays { get; set; }
    
      [JsonPropertyName("openIssues")]
      public int? OpenIssues { get; set; }
    
      [JsonPropertyName("resolvedEvents")]
      public int? ResolvedEvents { get; set; }
   }
}