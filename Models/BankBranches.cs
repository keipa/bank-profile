using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class BankBranches
   {
      [JsonPropertyName("count")]
      public required int Count { get; set; }
    
      [JsonPropertyName("countries")]
      public List<string>? Countries { get; set; }
    
      [JsonPropertyName("atmCount")]
      public int? AtmCount { get; set; }
    
      [JsonPropertyName("partnerAtmNetwork")]
      public int? PartnerAtmNetwork { get; set; }
   }
}