using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Core;

/// <summary>
/// Background service that periodically syncs locally cached entries to the distributed cache.
/// </summary>
internal class CacheEntryBackgroundService : BackgroundService
{
    private readonly MemoryCache<ManagedHubSession> _inMemoryCacheProvider;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger _logger;

public CacheEntryBackgroundService
(
    MemoryCache<ManagedHubSession> memoryCache,
    IDistributedCache cacheProvider,
    ILogger<CacheEntryBackgroundService> logger
)
{
    _inMemoryCacheProvider = memoryCache;
    _distributedCache = cacheProvider;
    _logger = logger;
}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            
            List<ManagedHubSession> entries = _inMemoryCacheProvider.List();

            foreach (var entry in entries)
            {
                try
                {
                    (string key, string value) = entry.ToKeyValuePair();
                    await _distributedCache.SetAsync(key, value, Constants.ManagedHubSessionCacheTtl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(message: $"Failed to write key {entry.ToKeyValuePair().Key} to distributed cache.\n" +
                                                $"Exception :\t {ex.Message}");
                }
            }
            await Task.Delay(Constants.ManagedHubSessionCacheReInstateInterval, stoppingToken);
        }
    }
}
