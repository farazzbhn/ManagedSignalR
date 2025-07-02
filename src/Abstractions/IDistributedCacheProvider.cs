namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Thread-safe cache for storing SignalR connection data
/// </summary>
public interface IDistributedCacheProvider
{
    /// <summary>
    /// Retrieves a value from cache. <br />
    /// <b>throws if connection cannot be established to the cache service</b>
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or null if non-existent</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Stores a value in cache <br />
    /// <b>throws if connection cannot be established to the cache service</b>
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to store</param>
    Task SetAsync<T>(string key, T value) where T : class;

    /// <summary>
    /// Removes a value from cache <br />
    /// <b>throws if connection cannot be established to the cache service</b>
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if value was removed</returns>
    Task<bool> RemoveAsync(string key);
}
