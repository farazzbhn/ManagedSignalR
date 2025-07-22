using System.Runtime.CompilerServices;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;

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
