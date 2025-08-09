using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Abstractions;

internal abstract class ManagedHubHelperBase<THub> : IManagedHubHelper<THub> where THub : ManagedHub
{
    protected IConnectionManager Connections { get; }

    protected ILogger<ManagedHubHelperBase<THub>> Logger;


    private readonly IHubContext<THub, IManagedHubClient> _context;


    public ManagedHubHelperBase
    (
        IConnectionManager<THub> connections, 
        ILogger<ManagedHubHelperBase<THub>> logger,
        IHubContext<THub, IManagedHubClient> context
    )
    {
        Connections = connections;
        Logger = logger;
        _context = context;
    }


    public abstract Task SendToUserAsync(object message, string? userIdentifier, int? maxConcurrency = null);

    public abstract Task SendToConnectionAsync(object message, string connectionId);


    protected (string Topic, string Payload) Serialize(dynamic message)
    {
        EndpointOptions configuration = FrameworkOptions.Instance.GetEndpointOptions(typeof(THub));

        if (!configuration.InvokeClientConfigurations.TryGetValue((Type) message.GetType(), out InvokeClientConfiguration? route))
            throw new MissingConfigurationException($"No configuration found for message type {typeof(MessageProcessingHandler)}. Please ensure it is registered with ConfigureInvokeClient<TModel>() method.");


        string topic = route.Topic!;
        string payload = route.Serialize(message);
        
        return (topic, payload);
    }


    /// <summary>
    /// Tries to invoke the <see cref=IManagedHubClientt.InvokeClient"/> on a SignalR connection.
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
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

}
