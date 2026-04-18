namespace BankProfiles.Web.Services
{
   public class CacheStatistics
   {
      public long CurrentSizeBytes { get; set; }
      public long SizeLimitBytes { get; set; }
      public int ItemCount { get; set; }
      public long Hits { get; set; }
      public long Misses { get; set; }
      public double HitRatio { get; set; }
    
      public double CurrentSizeMB => CurrentSizeBytes / (1024.0 * 1024.0);
      public double SizeLimitMB => SizeLimitBytes / (1024.0 * 1024.0);
   }
}