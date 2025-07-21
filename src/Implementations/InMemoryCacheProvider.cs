using Microsoft.Extensions.Caching.Memory;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="ICacheProvider"/> using Microsoft's <see cref="IMemoryCache"/>.
/// Supports TTL (Time-To-Live in milliseconds) via absolute expiration and key scanning via a tracked key registry.
/// </summary>
public class InMemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    // A thread-safe set of keys to support pattern-based scanning
    private static readonly ConcurrentDictionary<string, byte> _keys = new();

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
        var options = ttl_milliseconds is not null
            ? new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(ttl_milliseconds.Value),
                PostEvictionCallbacks = {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (_, k, _, _) => _keys.TryRemove(k.ToString()!, out _)
                    }
                }
            }
            : new MemoryCacheEntryOptions
            {
                PostEvictionCallbacks = {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (_, k, _, _) => _keys.TryRemove(k.ToString()!, out _)
                    }
                }
            };

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.FromResult(true);
    }

    public Task<string[]> ScanAsync(string pattern)
    {
        // Convert Redis-style pattern to a regex (e.g., "msr:faraz:*" => "^msr:faraz:.*$")
        string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";

        var regex = new Regex(regexPattern, RegexOptions.Compiled);
        string[] matchedKeys = _keys.Keys.Where(k => regex.IsMatch(k)).ToArray();
        return Task.FromResult(matchedKeys);
    }
}
