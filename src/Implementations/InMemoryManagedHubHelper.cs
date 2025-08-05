using System.Runtime.CompilerServices;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// SignalR message helper for single-instance deployments.
/// Uses local memory cache for connection tracking. No distributed cache required.
/// </summary>
/// <remarks>
/// <b>Use cases:</b> Development, testing, small applications, single-server deployments.
/// <b>Limitations:</b> No cross-instance messaging, data lost on restart.
/// </remarks>
internal class InMemoryManagedHubHelper : ManagedHubHelper
{
    private readonly ItemBag<UserConnectionGroup> _localCache;

    public InMemoryManagedHubHelper
    (
        ILogger<ManagedHubHelper> logger, 
        IServiceProvider serviceProvider, 
        ManagedSignalRConfiguration configuration, 
        ItemBag<UserConnectionGroup> localCache
    ) : base(logger, serviceProvider, configuration)
    {
        _localCache = localCache;
    }

    /// <summary>
    /// Sends message to specific connection ID.
    /// </summary>
    /// <exception cref="ArgumentNullException">connectionId null/empty or message null</exception>
    public override async Task SendToConnectionId<THub>(string connectionId, dynamic message)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentNullException(nameof(connectionId));

        ArgumentNullException.ThrowIfNull(message);

        (string Topic, string Payload) serialized = base.Serialize<THub>(message);
        bool result = await TryInvokeClient<THub>(connectionId, serialized.Topic, serialized.Payload);

        if (!result)
        {
            Logger.LogWarning("Failed to send message to connection '{ConnectionId}'", connectionId);
        }
    }

    /// <summary>
    /// Sends message to all connections of a user.
    /// </summary>
    /// <exception cref="ArgumentNullException">userId null/empty or message null</exception>
    public override async Task SendToUserId<THub>(string userId, dynamic message)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        ArgumentNullException.ThrowIfNull(message);

        // Get all user sessions from local cache
        IEnumerable<UserConnectionGroup> sessions = _localCache.List().Where(s => s.UserId == userId);
        IEnumerable<string> connectionIds = sessions.Select(s => s.ConnectionId);

        // Send to all connections concurrently
        (string Topic, string Payload) serialized = base.Serialize<THub>(message);
        IEnumerable<Task<bool>> tasks = connectionIds.Select(connId => 
            TryInvokeClient<THub>(connId, serialized.Topic, serialized.Payload));

        bool[] results = await Task.WhenAll(tasks);
        int successCount = results.Count(r => r);

        Logger.LogInformation("Message sent to {SuccessCount} of {TotalCount} connection(s) for userId '{UserId}'",
            successCount, results.Length, userId);
    }
}
