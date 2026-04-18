namespace BankProfiles.Web.Services
{
   public interface ICacheManager
   {
      T? Get<T>(string key);
      void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null);
      void Remove(string key);
      void Clear();
      CacheStatistics GetStatistics();
   }
}