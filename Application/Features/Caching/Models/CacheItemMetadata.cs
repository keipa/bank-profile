namespace BankProfiles.Web.Application.Features.Caching.Models;

public class CacheItemMetadata
{
   public required string Key { get; set; }
   public long Size { get; set; }
   public DateTime CreatedTime { get; set; }
   public DateTime LastAccessTime { get; set; }
   public int AccessCount { get; set; }
}
