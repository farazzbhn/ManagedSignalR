namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Thread-safe cache for storing SignalR connection data
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Retrieves a value from cache. <br />
    /// <b>throws if connection cannot be established to the cache service</b>
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or null if non-existent</returns>
    Task<string> GetAsync(string key);

    /// <summary>
    /// Stores a value in cache <br />
    /// <b>throws if connection cannot be established to the cache service</b>
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to store</param>
    /// <param name="ttl_milliseconds"></param>
    Task SetAsync(string key, string value, int? ttl_milliseconds = null);

    /// <summary>
    /// Removes a value from cache <br />
    /// <b>throws if connection cannot be established to the cache service</b>
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if value was removed</returns>
    Task<bool> RemoveAsync(string key);

    Task<String[]> Scan(string pattern);
}

