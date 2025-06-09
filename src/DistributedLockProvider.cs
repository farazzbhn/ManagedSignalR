using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR;
internal class DistributedLockProvider 
{
    private static readonly TimeSpan PollDelay = TimeSpan.FromMilliseconds(100);

    private readonly ICacheProvider _cacheProvider;

    public DistributedLockProvider(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    private static string GenKey(string key) => $"{nameof(DistributedLockProvider)}:{key}";

    /// <summary>
    /// Attempts to acquire a lock on the specified key.
    /// </summary>
    /// <param name="key">Unique lock key</param>
    /// <param name="timeout">How long to wait before giving up</param>
    /// <returns>The token if lock was acquired, otherwise null</returns>
    public async Task<string?> WaitAsync(string userId, TimeSpan? timeout = null)
    {
        string key = GenKey(userId);

        timeout ??= TimeSpan.FromSeconds(10);
        var token = Guid.NewGuid().ToString("N");
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            var existing = await _cacheProvider.GetAsync<string>(key);
            if (existing == null)
            {
                await _cacheProvider.SetAsync(key, token);
                // Re-check if we're still the owner (in case of race condition)
                var current = await _cacheProvider.GetAsync<string>(key);
                if (current == token) return token;
            }

            await Task.Delay(PollDelay);
        }

        return null; // Timed out
    }

    /// <summary>
    /// Releases the lock if token matches.
    /// </summary>
    /// <param name="userId">Lock key</param>
    /// <param name="token">Token that was returned by WaitAsync</param>
    /// <returns>True if lock was successfully released</returns>
    public async Task<bool> ReleaseAsync(string userId, string token)
    {
        string key = GenKey(userId);

        var current = await _cacheProvider.GetAsync<string>(key);
        if (current == token)
        {
            return await _cacheProvider.RemoveAsync(key);
        }
        return false; // Lock was taken or already released
    }

}
