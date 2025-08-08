using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Abstractions;

internal abstract class ManagedHubHelperBase<THub> : IManagedHubHelper<THub> where THub : AbstractManagedHub
{

    private readonly ManagedSignalRConfiguration _configuration;
    private readonly IHubContext<THub, IManagedHubClient> _context;
    private readonly ILogger<ManagedHubHelperBase<THub>> _logger;
    private readonly IConnectionTracker<THub> _connectionTracker;

    protected ManagedHubHelperBase
    (
        ManagedSignalRConfiguration configuration, 
        IHubContext<THub, IManagedHubClient> context,
        ILogger<ManagedHubHelperBase<THub>> logger, 
        IConnectionTracker<THub> connectionTracker
    )
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
        _connectionTracker = connectionTracker;
    }


    public Task<string[]> ListConnectionIdsAsync(string? userIdentifier)
    {
        return _connectionTracker.ListConnectionIdsAsync(userIdentifier);
    }

    public abstract Task SendToUserAsync(object message, string? userIdentifier, int? maxConcurrency = null);

    public abstract Task SendToConnectionIdAsync(object message, string connectionId);


    protected (string Topic, string Payload) Serialize(dynamic message)
    {
        EndpointConfiguration configuration = _configuration.FetchEndpointConfiguration(typeof(THub));

        if (!configuration.InvokeClientConfigurations.TryGetValue((Type) message.GetType(), out InvokeClientConfiguration? route))
            throw new MissingConfigurationException($"No configuration found for message type {typeof(MessageProcessingHandler)}. Please ensure it is registered with ConfigureInvokeClient<TModel>() method.");


        string topic = route.Topic!;
        string payload = route.Serialize(message);
        
        return (topic, payload);
    }


    /// <summary>
    /// Tries to invoke the <see cref="IManagedHubClient.InvokeClient"/> on a SignalR connection.
    /// </summary>
    /// <typeparam name="THub">The type of the SignalR hub.</typeparam>
    /// <param name="connectionId">The target SignalR connection ID.</param>
    /// <param name="topic">The topic name to invoke on the client.</param>
    /// <param name="payload">The serialized message payload.</param>
    /// <returns><c>true</c> if the client invocation succeeded; otherwise, <c>false</c>.</returns>
    internal async Task<bool> TryInvokeClientAsync
    (
        string connectionId,
        string topic,
        string payload
    )
    {
        try
        {
            await _context.Clients.Client(connectionId).InvokeClient(topic, payload);

            _logger.LogDebug("Successfully sent message to local connection {ConnectionId}", connectionId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to local connection {ConnectionId} : {Error}", connectionId, ex.Message);
            return false;
        }
    }

}
