using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// SignalR message helper for single-instance deployments.
/// Uses local memory cache for connection tracking. no pub-sub pattern is used.
/// </summary>
/// <remarks>
/// <b>Use cases:</b> Development, testing, small applications, single-server deployments.
/// <b>Limitations:</b> No messaging, data lost on restart.
/// </remarks>
internal class InMemoryManagedHubHelper : ManagedHubHelper
{
    private readonly IUserConnectionCache _userConnectionCache;

    public InMemoryManagedHubHelper
    (
        ILogger<ManagedHubHelper> logger, 
        IServiceProvider serviceProvider, 
        ManagedSignalRConfiguration configuration, 
        IUserConnectionCache userConnectionCache
    ) : base(logger, serviceProvider, configuration)
    {
        _userConnectionCache = userConnectionCache;
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
    public override async Task SendToUserId<THub>(string? userIdentifier, dynamic message)
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
            throw new ArgumentNullException(nameof(userIdentifier));

        ArgumentNullException.ThrowIfNull(message);

        // retrieve using the configuration the topic and payload for the message
        (string Topic, string Payload) serialized = base.Serialize<THub>(message);

        // Send to all connections concurrently
        string[] connectionIds = _userConnectionCache.GetUserConnections(typeof(THub), userIdentifier).Select(uc => uc.ConnectionId).ToArray();
        IEnumerable<Task<bool>> tasks = connectionIds.Select(connId => TryInvokeClient<THub>(connId, serialized.Topic, serialized.Payload));

        bool[] results = await Task.WhenAll(tasks);
        int successCount = results.Count(r => r);

        Logger.LogInformation("Message sent to {SuccessCount} of {TotalCount} connection(s) for userId '{UserId}'",
            successCount, results.Length, userIdentifier);
    }
}
