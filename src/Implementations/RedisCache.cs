using ManagedLib.ManagedSignalR.Abstractions;
using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// Redis-based implementation of <see cref="IDistributedCache"/>.
/// Supports TTL (Time-To-Live in milliseconds) via key expiration and pattern-based scanning.
/// </summary>
public class RedisCache : IDistributedCache
{
    private readonly IDatabase _db;
    private readonly IServer _server;

    public RedisCache(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();

        // Select a server endpoint for key scanning
        var endpoint = redis.GetEndPoints().First();
        _server = redis.GetServer(endpoint);
    }

    public async Task<string?> GetAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetAsync(string key, string value, int? ttl_milliseconds = null)
    {
        TimeSpan? expiry = ttl_milliseconds.HasValue
            ? TimeSpan.FromMilliseconds(ttl_milliseconds.Value)
            : (TimeSpan?)null;

        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    public Task<string[]> ScanAsync(string pattern)
    {
        // Convert wildcard to Redis-compatible pattern for server-side scan
        // e.g., "msr:faraz:*"
        var keys = _server.Keys(pattern: pattern).Select(k => k.ToString()).ToArray();
        return Task.FromResult(keys);
    }
}