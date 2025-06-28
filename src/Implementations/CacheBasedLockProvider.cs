using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Implementations;


/// <summary>
/// A distributed lock implementation which relies on the <see cref="ICacheProvider"/> to function
/// </summary>
internal class CacheBasedLockProvider : ILockProvider
{
    private readonly ICacheProvider _cacheProvider;

    public CacheBasedLockProvider(ICacheProvider cacheProvider) 
    {
        _cacheProvider = cacheProvider;
    }


    // prefixes the cache key to ensure uniqueness of teh key
    private static string Prefix(string key) => $"{nameof(CacheBasedLockProvider)}:{key}";


    public async Task<string?> WaitAsync(string key, TimeSpan? timeout = null)
    {

        string prefixed = Prefix(key);

        timeout ??= TimeSpan.FromSeconds(10);
        var token = Guid.NewGuid().ToString("N");
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            var existing = await _cacheProvider.GetAsync<string>(prefixed);

            if (existing == null)
            {
                await _cacheProvider.SetAsync(prefixed, token);
                // Re-check if we're still the owner (in case of race condition)
                var current = await _cacheProvider.GetAsync<string>(prefixed);
                if (current == token) return token;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        return null; // Timed out
    }




    public async Task<bool> ReleaseAsync(string key, string token)
    {
        string prefixed = Prefix(key);

        var current = await _cacheProvider.GetAsync<string>(prefixed);

        if (current == token)  return await _cacheProvider.RemoveAsync(prefixed);

        return false; 
    }

}
