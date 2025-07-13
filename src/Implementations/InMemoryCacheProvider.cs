using Microsoft.Extensions.Caching.Memory;
using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IDistributedCacheProvider"/> using Microsoft's IMemoryCache.
/// Supports TTL (Time-To-Live) via absolute expiration.
/// </summary>
public class InMemoryCacheProvider : IDistributedCacheProvider
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return Task.FromResult(value as T);
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.FromResult(true);
    }
}