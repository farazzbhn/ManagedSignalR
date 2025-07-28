using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;

internal class DistributedManagedHubHelper : ManagedHubHelper
{
    private readonly IDistributedCacheProvider _cacheProvider;
    private readonly LocalCacheProvider<ManagedHubSessionCacheEntry> _localCacheProvider;
    private readonly IEnvelopePublishEndpoint _publishEndpoint;

    public DistributedManagedHubHelper
    (
        ILogger<ManagedHubHelper> logger, 
        IServiceProvider serviceProvider, 
        ManagedSignalRConfiguration configuration, 
        IDistributedCacheProvider cacheProvider,
        LocalCacheProvider<ManagedHubSessionCacheEntry> localCacheProvider, 
        IEnvelopePublishEndpoint publishEndpoint
    ) : base(logger, serviceProvider, configuration)
    {
        _cacheProvider = cacheProvider;
        _localCacheProvider = localCacheProvider;
        _publishEndpoint = publishEndpoint;
    }

    public async Task SendToConnectionId<THub>(string userId, string connectionId, dynamic message)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentNullException(nameof(connectionId));

        ArgumentNullException.ThrowIfNull(message);

        (string Topic, string Payload) serialized = Serialize(message);

        bool owned = _localCacheProvider.List().Any(x => x.Session.ConnectionId == connectionId);
    }

    public override async Task SendToConnectionId<THub>(string connectionId, dynamic message)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentNullException(nameof(connectionId));

        ArgumentNullException.ThrowIfNull(message);

        (string Topic, string Payload) serialized = Serialize(message);

        // decide if the connection belongs to this very instance 
        bool owned = _localCacheProvider.List().Any(x => x.Session.ConnectionId == connectionId);

        // the connection id belongs to this very instance => Try to invoke the client directly 
        if (owned)
        {
            bool result = await TryInvokeClient<THub>(connectionId, serialized.Topic, serialized.Payload);

            if (!result)
            {
                Logger.LogWarning("Failed to send message to connection '{ConnectionId}'", connectionId);
            }
            return;
        }
        else // the connection belongs to another instance of the application
        {
            string[] keys = await _cacheProvider.ScanAsync($"msr:*:{connectionId}");

            if (keys.Any())
            {
                var envelope = new Envelope()
                {
                    ConnectionId = connectionId,
                    Payload = serialized.Payload,
                    Topic = serialized.Topic,
                    InstanceId = "dsa"
                };

            }
            else
            {
                Logger.LogWarning("Failed to send message to connection '{ConnectionId}'", connectionId);

            }
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

        (string Topic, string Payload) serialized = Serialize(message);
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
