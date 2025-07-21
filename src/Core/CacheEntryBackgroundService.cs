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
    private readonly LocalCacheProvider<CacheEntry> _localCacheProvider;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger _logger;

public CacheEntryBackgroundService
(
    LocalCacheProvider<CacheEntry> localCacheProvider,
    ICacheProvider cacheProvider,
    ILogger<CacheEntryBackgroundService> logger
)
{
    _localCacheProvider = localCacheProvider;
    _cacheProvider = cacheProvider;
    _logger = logger;
}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            List<CacheEntry> entries = _localCacheProvider.List();
            foreach (var entry in entries)
            {
                try
                {
                    await _cacheProvider.SetAsync(entry.key, entry.value, Constants.ManagedHubSessionCacheTtl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(message: $"Failed to write key {entry.key} to distributed cache.\n" +
                                                $"Exception :\t {ex.Message}");
                }
            }
            await Task.Delay(Constants.ManagedHubSessionCacheReInstateInterval, stoppingToken);
        }
    }
}
