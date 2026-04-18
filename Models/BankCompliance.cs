using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankCompliance
   {
      [JsonPropertyName("sanctionsRisk")]
      public required string SanctionsRisk { get; set; } // low, moderate, high, critical, unknown
    
      [JsonPropertyName("governmentAffiliate")]
      public bool? GovernmentAffiliate { get; set; }
    
      [JsonPropertyName("pepExposure")]
      public string? PepExposure { get; set; } // low, moderate, high, unknown
    
      [JsonPropertyName("offshoreLinks")]
      public string? OffshoreLinks { get; set; } // none, limited, moderate, significant, unknown
    
      [JsonPropertyName("amlStatus")]
      public required string AmlStatus { get; set; } // good, watch, review, critical, unknown
    
      [JsonPropertyName("kycStatus")]
      public required string KycStatus { get; set; } // complete, partial, weak, unknown
    
      [JsonPropertyName("fATCA")]
      public bool? FATCA { get; set; }
    
      [JsonPropertyName("crs")]
      public bool? CRS { get; set; }
    
      [JsonPropertyName("auditPublished")]
      public bool? AuditPublished { get; set; }
    
      [JsonPropertyName("depositInsurance")]
      public bool? DepositInsurance { get; set; }
   }
}