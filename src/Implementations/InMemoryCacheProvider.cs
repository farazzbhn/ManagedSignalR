using System;
using System.Collections.Concurrent;
using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IDistributedCacheProvider"/> using ConcurrentDictionary.
/// This is the default implementation suitable for single-server scenarios.
/// For distributed systems, consider implementing a distributed cache provider and mind the uniqueness of the key
/// </summary>
public class InMemoryCacheProvider : IDistributedCacheProvider
{
    private static readonly ConcurrentDictionary<string, object> s_cache = new();

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (s_cache.TryGetValue(key, out var value))
        {
            return Task.FromResult(value as T);
        }
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value) where T : class
    {
        s_cache[key] = value;
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string key)
    {
        return Task.FromResult(s_cache.TryRemove(key, out _));
    }
}
