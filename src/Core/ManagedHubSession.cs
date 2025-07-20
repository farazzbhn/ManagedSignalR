namespace ManagedLib.ManagedSignalR.Core;


/// <summary>
/// Represents a managed session for a SignalR hub connection,
/// including the user ID, connection ID, and the associated instance ID.
/// </summary>
internal class ManagedHubSession
{
    private const string prefix = "msr";

    /// <summary>
    /// Gets or sets the user identifier associated with the session.
    /// </summary>
    internal string UserId { get; private set; }

    /// <summary>
    /// Gets or sets the SignalR connection identifier for the session.
    /// </summary>
    internal string ConnectionId { get; private set; }

    /// <summary>
    /// Gets or sets the instance identifier associated with the session (usually the cache value).
    /// </summary>
    internal string InstanceId { get; private set; }


    internal ManagedHubSession(string userId, string connectionId, string instanceId)
    {
        UserId = userId;
        ConnectionId = connectionId;
        InstanceId = instanceId;
    }

    /// <summary>
    /// Parses a cache key and value into a <see cref="ManagedHubSession"/> instance.
    /// </summary>
    /// <param name="key">
    /// The cache key in the format <c>"userId:connectionId"</c>.
    /// </param>
    /// <param name="value">
    /// The value associated with the cache key (e.g., the instance ID).
    /// </param>
    /// <returns>A populated <see cref="ManagedHubSession"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
    /// <exception cref="FormatException">Thrown if <paramref name="key"/> is not in the expected format.</exception>
    internal static ManagedHubSession FromCacheKeyValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        var parts = key.Split(':', StringSplitOptions.None);

        if (parts.Length != 3 || parts[0] != prefix)
            throw new FormatException($"Cache key must be in the format '{prefix}:userId:connectionId'");

        return new ManagedHubSession
        (
            userId: parts[1],
            connectionId: parts[2],
            instanceId: value
        );
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
    internal (string Key, string Value) ToCacheKeyValue()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(UserId));

        if (string.IsNullOrWhiteSpace(ConnectionId))
            throw new ArgumentException("ConnectionId cannot be null or empty.", nameof(ConnectionId));

        if (string.IsNullOrWhiteSpace(InstanceId))
            throw new ArgumentException("InstanceId cannot be null or empty.", nameof(InstanceId));

        string key = $"{prefix}:{UserId}:{ConnectionId}";
        string value = InstanceId;

        return (key, value);
    }


}
