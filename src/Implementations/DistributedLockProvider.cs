using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;


/// <summary>
/// A distributed lock implementation which relies on the <see cref="IDistributedCacheProvider"/> to function
/// </summary>
internal class DistributedLockProvider : IDistributedLockProvider
{
    private readonly IDistributedCacheProvider _disributedCacheProvider;
    private readonly ILogger _logger;

    public DistributedLockProvider
    (
        IDistributedCacheProvider distributedCache, 
        ILogger logger
    )
    {
        _disributedCacheProvider = distributedCache;
        _logger = logger;
    }

    /// <summary>
    /// prefixes the cache key to ensure uniqueness of the key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static string KeyGen(string key) => $"{Constants.CacheKeyPrefix}lock:{key}";


    public async Task<string?> AcquireAsync(string userId, TimeSpan? timeout = null)
    {

        string key = KeyGen(userId);

        timeout ??= TimeSpan.FromSeconds(10);
        var token = Guid.NewGuid().ToString("N");
        var startTime = DateTime.UtcNow;

        try
        {

            while (DateTime.UtcNow - startTime < timeout)
            {
                string? existing = await _disributedCacheProvider.GetAsync(key);

                if (existing == null)
                {
                    await _disributedCacheProvider.SetAsync(key, token, Constants.LockTTL);
                    // Re-check if we're still the owner (in case of race condition)
                    //var current = await _disributedCacheProvider.GetAsync<string>(cacheKey);
                    //if (current == token)
                    return token;
                }

                //  try again in 0.1 seconds
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            // lock not acquired && no exception thrown ?
            _logger.LogError("Failed to acquire lock using key {key}", key);
            return null;
        }
        catch(Exception ex) 
        {
            _logger.LogError("Failed to acquire lock using key {key}\nException : {ex}", key, ex.Message);
            return null; // Timed out
        }
    }




    public async Task<bool> ReleaseAsync(string userId, string expectedToken)
    {
        string key = KeyGen(userId);
        try
        {
            string? storedToken = await _disributedCacheProvider.GetAsync(key);

            if (storedToken == expectedToken)
            {
                return await _disributedCacheProvider.RemoveAsync(key);
            }
            // expired already
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to perform Read/Write operation in the registered cache provider\n" +
                             $"The acquired lock \"{key}\" is expected to automatically expire in {Constants.LockTTL} milliseconds\n"+
                             $"\nException : \t {e.Message}");
            return false;
        }
    }

}
