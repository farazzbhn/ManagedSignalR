namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Thread-safe cache for storing SignalR connection data
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Retrieves a value from cache
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or null</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Stores a value in cache
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to store</param>
    Task SetAsync<T>(string key, T value) where T : class;

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if value was removed</returns>
    Task<bool> RemoveAsync(string key);
}
