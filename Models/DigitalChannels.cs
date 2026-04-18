using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models
{
   public class DigitalChannels
   {
      [JsonPropertyName("mobileApp")]
      public bool? MobileApp { get; set; }
    
      [JsonPropertyName("webBanking")]
      public bool? WebBanking { get; set; }
    
      [JsonPropertyName("ios")]
      public bool? Ios { get; set; }
    
      [JsonPropertyName("android")]
      public bool? Android { get; set; }
    
      [JsonPropertyName("apiAccess")]
      public bool? ApiAccess { get; set; }
    
      [JsonPropertyName("biometricLogin")]
      public bool? BiometricLogin { get; set; }
    
      [JsonPropertyName("deviceTrust")]
      public bool? DeviceTrust { get; set; }
    
      [JsonPropertyName("pushNotifications")]
      public bool? PushNotifications { get; set; }
    
      [JsonPropertyName("uptimePercent")]
      public double? UptimePercent { get; set; }
    
      [JsonPropertyName("averageAccountOpeningMinutes")]
      public double? AverageAccountOpeningMinutes { get; set; }
   }
}