using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace BankProfiles.Web.Services;

public class CacheManager : ICacheManager
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, CacheItemMetadata> _metadata;
    private long _currentSizeBytes;
    private readonly long _sizeLimitBytes;
    private long _hits;
    private long _misses;

    public CacheManager(IMemoryCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
        _metadata = new ConcurrentDictionary<string, CacheItemMetadata>();

        var sizeLimitMB = _configuration.GetValue<int>("CacheSettings:SizeLimitMB");
        _sizeLimitBytes = (long)sizeLimitMB * 1024 * 1024;
    }

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            // Update access time for LRU
            if (_metadata.TryGetValue(key, out var metadata))
            {
                metadata.LastAccessTime = DateTime.UtcNow;
                metadata.AccessCount++;
            }

            Interlocked.Increment(ref _hits);
            return value;
        }

        Interlocked.Increment(ref _misses);
        return default;
    }

    public void Set<T>(string key, T value, TimeSpan? absoluteExpiration = null)
    {
        if (value == null) return;

        var estimatedSize = EstimateSize(value);

        // Check if we need to evict items
        while (_currentSizeBytes + estimatedSize > _sizeLimitBytes && _metadata.Any())
        {
            EvictLeastRecentlyUsed();
        }

        var expiration = absoluteExpiration ??
            TimeSpan.FromMinutes(_configuration.GetValue<int>("CacheSettings:AbsoluteExpirationMinutes"));

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            Size = estimatedSize
        };

        cacheOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            if (_metadata.TryRemove(key.ToString()!, out var metadata))
            {
                Interlocked.Add(ref _currentSizeBytes, -metadata.Size);
            }
        });

        _cache.Set(key, value, cacheOptions);

        _metadata[key] = new CacheItemMetadata
        {
            Key = key,
            Size = estimatedSize,
            CreatedTime = DateTime.UtcNow,
            LastAccessTime = DateTime.UtcNow,
            AccessCount = 0
        };

        Interlocked.Add(ref _currentSizeBytes, estimatedSize);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _metadata.TryRemove(key, out _);
    }

    public void Clear()
    {
        foreach (var key in _metadata.Keys.ToList())
        {
            _cache.Remove(key);
        }
        _metadata.Clear();
        Interlocked.Exchange(ref _currentSizeBytes, 0);
    }

    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            CurrentSizeBytes = _currentSizeBytes,
            SizeLimitBytes = _sizeLimitBytes,
            ItemCount = _metadata.Count,
            Hits = _hits,
            Misses = _misses,
            HitRatio = _hits + _misses > 0 ? (double)_hits / (_hits + _misses) : 0
        };
    }

    private void EvictLeastRecentlyUsed()
    {
        var lruItem = _metadata.Values
            .OrderBy(m => m.LastAccessTime)
            .ThenBy(m => m.AccessCount)
            .FirstOrDefault();

        if (lruItem != null)
        {
            Remove(lruItem.Key);
        }
    }

    private long EstimateSize(object obj)
    {
        // Simple size estimation based on type
        return obj switch
        {
            string str => str.Length * 2, // Unicode characters
            _ => 1024 // Default 1KB for objects
        };
    }
}
