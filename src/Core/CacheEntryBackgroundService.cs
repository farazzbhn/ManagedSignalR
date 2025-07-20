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
    private readonly IDistributedCacheProvider _distributedCacheProvider;
    private readonly ILogger _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(2000);

public CacheEntryBackgroundService
(
    LocalCacheProvider<CacheEntry> localCacheProvider,
    IDistributedCacheProvider distributedCacheProvider,
    ILogger<CacheEntryBackgroundService> logger
)
{
    _localCacheProvider = localCacheProvider;
    _distributedCacheProvider = distributedCacheProvider;
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
                    await _distributedCacheProvider.SetAsync(entry.key, entry.value, Constants.SessionTtl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(message: $"Failed to write key {entry.key} to distributed cache.\n" +
                                                $"Exception :\t {ex.Message}");
                }
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
