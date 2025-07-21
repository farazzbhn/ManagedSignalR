using Microsoft.Extensions.Caching.Memory;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="ICacheProvider"/> using Microsoft's IMemoryCache.
/// Supports TTL (Time-To-Live (ms)) via absolute expiration.
/// </summary>
public class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }


    public Task<string?> GetAsync(string key) 
    {

        if (_cache.TryGetValue(key, out string? value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult<string?>(null);
    }

    public Task SetAsync(string key, string value, int? ttl_milliseconds = null)
    {
        var options = ttl_milliseconds is not null ? 
            new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(ttl_milliseconds.Value)} :
            null;

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.FromResult(true);
    }
}