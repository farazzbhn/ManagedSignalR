using System.Runtime.CompilerServices;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;


/// <summary>
/// A helper class for managing SignalR hubs in a single-instance architecture.
/// <para>
/// This class extends <see cref="ManagedHubHelper"/> and facilitates sending messages to clients
/// by connection ID or user ID within a single-instance SignalR server environment.
/// </para>
/// </summary>
/// <remarks>
/// <para><b>Architecture:</b></para>
/// <para>
/// In a single-instance setup, all relevant state such as client connections, sessions, and
/// message routing information is stored locally within the server process. This means:
/// </para>
/// <list type="bullet">
///   <item><description>There is no distributed cache or external session store involved.</description></item>
///   <item><description>Cache keys and session data are maintained in-process or in local memory cache.</description></item>
///   <item><description>Message dispatching targets clients connected to this single instance only.</description></item>
///   <item><description>This design avoids complexities of cross-instance synchronization or message forwarding.</description></item>
/// </list>
/// <para>
/// This simplifies the implementation but also means the architecture is suited for scenarios
/// where scaling out (multiple server instances) is not required or handled separately.
/// </para>
/// <para><b>Presumptions and dependencies:</b></para>
/// <list type="bullet">
///   <item><description>Relies on <see cref="ICacheProvider"/> to track connection/session keys locally.</description></item>
///   <item><description>Sessions are created from locally cached keys following a specific naming convention.</description></item>
///   <item><description>Uses logging and serialization services inherited from <see cref="ManagedHubHelper"/>.</description></item>
/// </list>
/// <para><b>Main responsibilities:</b></para>
/// <list type="bullet">
///   <item><description>Sending messages to clients by connection ID or user ID within the single instance.</description></item>
///   <item><description>Performing local cache scans to find all connections related to a user.</description></item>
///   <item><description>Logging the results of message delivery attempts for monitoring and troubleshooting.</description></item>
/// </list>
/// </remarks>
internal class SingleInstanceManagedHubHelper : ManagedHubHelper
{
    private readonly ICacheProvider _cacheProvider;

    public SingleInstanceManagedHubHelper
    (
        ILogger<ManagedHubHelper> logger, 
        IServiceProvider serviceProvider, 
        ManagedSignalRConfiguration configuration, 
        ICacheProvider cacheProvider
    ) : base(logger, serviceProvider, configuration)
    {
        _cacheProvider = cacheProvider;
    }


    public override async Task SendToConnectionId<THub>(string connectionId, dynamic message)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentNullException(nameof(connectionId));

        ArgumentNullException.ThrowIfNull(message);

        (string Topic, string Payload) serialized = Serialize<THub>(message);

        // Try to invoke the client directly
        bool result = await TryInvokeClient<THub>(connectionId, serialized.Topic, serialized.Payload);

        if (!result)
        {
            Logger.LogWarning("Failed to send message to connection '{ConnectionId}'", connectionId);
        }
    }

    public override async Task SendToUserId<THub>(string userId, dynamic message)
    {

        // Input validation
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        ArgumentNullException.ThrowIfNull(message);

        // retrieve the list of cached keys to follow the msr:userId pattern
        string[] keys = await _cacheProvider.ScanAsync($"msr:{userId}:*");

        // Create respective sessions from the cached key/value pairs. 
        // The value (set within the ManagedHubSession) is in fact the instance id which corresponds
        // to the single instance currently running 
        IEnumerable<ManagedHubSession> sessions = keys.Select(k => ManagedHubSession.FromCacheKeyValue(k, AppInfo.InstanceId));

        // retrieve the list of connection ids 
        IEnumerable<string> connectionIds = sessions.Select(s => s.ConnectionId);


        // find the configuration for the hub type and serialize the message 

        (string Topic, string Payload) serialized = Serialize<THub>(message);
        // Invoke all clients concurrently

        IEnumerable<Task<bool>> tasks = connectionIds.Select(connId =>
            TryInvokeClient<THub>(connId, serialized.Topic, serialized.Payload));

        bool[] results = await Task.WhenAll(tasks);

        // Optionally: log the number of successes/failures
        int successCount = results.Count(r => r);

        Logger.LogInformation("Message sent to {SuccessCount} of {TotalCount} connection(s) for userId '{UserId}'",
            successCount, results.Length, userId);
    }

}
