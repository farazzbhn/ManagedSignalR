using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Implementations;


/// <summary>
/// A distributed lock implementation which relies on the <see cref="IDistributedCacheProvider"/> to function
/// </summary>
internal class DistributedLockProvider : IDistributedLockProvider
{
    private readonly IDistributedCacheProvider _disributedCacheProvider;

    public DistributedLockProvider(IDistributedCacheProvider distributedCache) 
    {
        _disributedCacheProvider = distributedCache;
    }


    // prefixes the cache key to ensure uniqueness of teh key
    private static string GenerateCacheKey(string key) => $"msrlock:{key}";


    public async Task<string?> WaitAsync(string key, TimeSpan? timeout = null)
    {

        string cacheKey = GenerateCacheKey(key);

        timeout ??= TimeSpan.FromSeconds(10);
        var token = Guid.NewGuid().ToString("N");
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            var existing = await _disributedCacheProvider.GetAsync<string>(cacheKey);

            if (existing == null)
            {
                await _disributedCacheProvider.SetAsync(cacheKey, token);
                // Re-check if we're still the owner (in case of race condition)
                var current = await _disributedCacheProvider.GetAsync<string>(cacheKey);
                if (current == token) return token;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        return null; // Timed out
    }




    public async Task<bool> ReleaseAsync(string key, string token)
    {
        string cacheKey = GenerateCacheKey(key);

        string? current = await _disributedCacheProvider.GetAsync<string>(cacheKey);

        if (current == token)  return await _disributedCacheProvider.RemoveAsync(cacheKey);

        return false; 
    }

}
