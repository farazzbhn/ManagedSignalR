namespace ManagedLib.ManagedSignalR.Abstractions;

public interface ICacheProvider
{
    /// <summary>
    /// Gets a value from the cache by key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value if found, otherwise default(T).</returns>
    T? Get<T>(string key) where T : class;

    /// <summary>
    /// Sets a value in the cache with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    void Set<T>(string key, T value) where T : class;

    /// <summary>
    /// Removes a value from the cache by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>True if the value was removed, false if it didn't exist.</returns>
    bool Remove(string key);
}
