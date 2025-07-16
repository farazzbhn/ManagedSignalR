namespace ManagedLib.ManagedSignalR.Types;


/// <summary>
/// Represents a managed session for a SignalR hub connection,
/// including the user ID, connection ID, and the associated instance ID.
/// </summary>
public class ManagedHubSession
{
    /// <summary>
    /// Gets or sets the user identifier associated with the session.
    /// </summary>
    public string UserId { get; protected set; }

    /// <summary>
    /// Gets or sets the SignalR connection identifier for the session.
    /// </summary>
    public string ConnectionId { get; protected set; }

    /// <summary>
    /// Gets or sets the instance identifier associated with the session (usually the cache value).
    /// </summary>
    public string InstanceId { get; protected set; }


    public ManagedHubSession(string userId, string connectionId, string instanceId)
    {
        UserId = userId;
        ConnectionId = connectionId;
        InstanceId = instanceId;
    }

    /// <summary>
    /// Parses a cache key and value into a <see cref="ManagedHubSession"/> instance.
    /// </summary>
    /// <param name="cacheKey">
    /// The cache key in the format <c>"userId:connectionId"</c>.
    /// </param>
    /// <param name="cacheValue">
    /// The value associated with the cache key (e.g., the instance ID).
    /// </param>
    /// <returns>A populated <see cref="ManagedHubSession"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="cacheKey"/> is null or empty.</exception>
    /// <exception cref="FormatException">Thrown if <paramref name="cacheKey"/> is not in the expected format.</exception>
    internal static ManagedHubSession Parse(string cacheKey, string cacheValue)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(cacheKey));

        var parts = cacheKey.Split(':', StringSplitOptions.None);

        if (parts.Length != 2)
            throw new FormatException("Cache key must be in the format 'userId:connectionId'");

        return new ManagedHubSession
        {
            UserId = parts[0],
            ConnectionId = parts[1],
            InstanceId = cacheValue
        };
    }



    /// <summary>
    /// Generates the cache key and value for a <see cref="ManagedHubSession"/> instance.
    /// </summary>
    /// <param name="session">The session to generate the cache entry for.</param>
    /// <returns>
    /// A tuple where:
    /// <list type="bullet">
    /// <item><description><c>Item1</c> is the key in the format <c>"userId:connectionId"</c></description></item>
    /// <item><description><c>Item2</c> is the value, which is the <c>InstanceId</c></description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="session"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="session"/> has any null or empty required properties.</exception>
    internal (string Key, string Value) ToCacheEntry()
    {

        if (string.IsNullOrWhiteSpace(UserId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(UserId));

        if (string.IsNullOrWhiteSpace(ConnectionId))
            throw new ArgumentException("ConnectionId cannot be null or empty.", nameof(ConnectionId));

        if (string.IsNullOrWhiteSpace(InstanceId))
            throw new ArgumentException("InstanceId cannot be null or empty.", nameof(InstanceId));

        var key = $"{UserId}:{ConnectionId}";
        var value = InstanceId;

        return (key, value);
    }

}
