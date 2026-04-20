using System.Text.Json.Serialization;
using BankProfiles.Web.Domain.Common.Metrics;

namespace BankProfiles.Web.Domain.BankProfiles;

public class BankClients
{
   [JsonPropertyName("total")]
   public required int Total { get; set; }
    
   [JsonPropertyName("retail")]
   public int? Retail { get; set; }
    
   [JsonPropertyName("business")]
   public int? Business { get; set; }
    
   [JsonPropertyName("corporate")]
   public int? Corporate { get; set; }
    
   [JsonPropertyName("privateBanking")]
   public int? PrivateBanking { get; set; }
    
   [JsonPropertyName("growthHistory")]
   public List<TimestampedMetric>? GrowthHistory { get; set; }
}