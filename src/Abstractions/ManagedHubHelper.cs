using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Abstract base class for helpers that send messages through SignalR hubs.
/// </summary>
public abstract class ManagedHubHelper
{
    protected readonly ILogger<ManagedHubHelper> Logger;

    private readonly IServiceProvider _serviceProvider;
    private readonly ManagedSignalRConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedHubHelper"/> class.
    /// </summary>
    /// <param name="serviceProvider">The dependency injection service provider.</param>
    /// <param name="logger">The logger instance for diagnostic purposes.</param>
    /// <param name="configuration">The configuration used for hub serialization.</param>
    protected ManagedHubHelper
    (
        ILogger<ManagedHubHelper> logger,
        IServiceProvider serviceProvider,
        ManagedSignalRConfiguration configuration
    )
    {
        Logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Sends a message to a specific user identified by user ID.
    /// </summary>
    /// <typeparam name="THub">The type of the SignalR hub.</typeparam>
    /// <param name="userId">The target user ID.</param>
    /// <param name="message">The message to send.</param>
    public abstract Task SendToUserId<THub>(string userId, dynamic message) where THub : ManagedHub;

    /// <summary>
    /// Sends a message to a specific connection identified by connection ID.
    /// </summary>
    /// <typeparam name="THub">The type of the SignalR hub.</typeparam>
    /// <param name="connectionId">The target connection ID.</param>
    /// <param name="message">The message to send.</param>
    public abstract Task SendToConnectionId<THub>(string connectionId, dynamic message) where THub : ManagedHub;

    /// <summary>
    /// Serializes a dynamic message using the configuration associated with the given hub type.
    /// </summary>
    /// <typeparam name="THub">The type of the SignalR hub.</typeparam>
    /// <param name="message">The message to serialize.</param>
    /// <returns>A tuple containing the topic and the serialized payload.</returns>
    protected (string Topic, string Payload) Serialize<THub>(dynamic message) where THub : ManagedHub
    {
        HubEndpointOptions config = _configuration.GetHubEndpointOptions(typeof(THub));
        (string Topic, string Payload) serialized = config.Serialize(message);
        return (serialized.Topic, serialized.Payload);
    }

    /// <summary>
    /// Tries to invoke the <see cref="IManagedHubClient.InvokeClient"/> on a SignalR connection.
    /// </summary>
    /// <typeparam name="THub">The type of the SignalR hub.</typeparam>
    /// <param name="connectionId">The target SignalR connection ID.</param>
    /// <param name="topic">The topic name to invoke on the client.</param>
    /// <param name="payload">The serialized message payload.</param>
    /// <returns><c>true</c> if the client invocation succeeded; otherwise, <c>false</c>.</returns>
    internal async Task<bool> TryInvokeClient<THub>(
        string connectionId,
        string topic,
        string payload
    ) where THub : ManagedHub
    {
        try
        {
            IHubContext<THub, IManagedHubClient>? context =
                _serviceProvider.GetRequiredService<IHubContext<THub, IManagedHubClient>>();

            if (context == null)
                throw new ServiceNotRegisteredException(
                    $"{typeof(IHubContext<,>).MakeGenericType(typeof(THub), typeof(IManagedHubClient)).FullName}"
                );

            await context.Clients.Client(connectionId).InvokeClient(topic, payload);

            Logger.LogDebug("Successfully sent message to connection {ConnectionId}", connectionId);

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to send message to connection {ConnectionId} : {Error}", connectionId, ex.Message);
            return false;
        }
    }
}
