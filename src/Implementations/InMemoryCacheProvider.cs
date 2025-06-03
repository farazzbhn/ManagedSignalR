using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Helper;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// Thread-safe in-memory implementation of ICacheProvider using ConcurrentDictionary.
/// This is the default implementation suitable for single-server scenarios.
/// For distributed systems, consider implementing a distributed cache provider.
/// </summary>
public class InMemoryCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public T? Get<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return value as T;
        }
        return null;
    }

    public void Set<T>(string key, T value) where T : class
    {
        _cache[key] = value;
    }

    public bool Remove(string key)
    {
        return _cache.TryRemove(key, out _);
    }
}
