using ManagedLib.ManagedSignalR.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// A ready-to-use SignalR hub base class that simplifies real-time communication between clients and server. <br/><br/>
/// <b>How to use:</b> <br/><br/>
/// 1. <b>Connection Management:</b> <br/>
///    - Inherit from this class to automatically handle client connections and disconnections <br/>
///    - Override <see cref="OnConnectedHookAsync"/> to run custom logic when clients connect <br/>
///    - Override <see cref="OnDisconnectedHookAsync"/> to clean up resources when clients disconnect <br/><br/>
/// 2. <b>Message Handling:</b> <br/>
///    - Clients send messages using the <b><see cref="Process"/></b> method with a topic and JSON payload <br/>
///    - Messages are <b>automatically deserialized</b> based on your topic configuration <br/>
///    - No need to write manual message parsing - just configure your topic-to-command mappings <br/><br/>
/// 3. <b>Command Processing:</b> <br/>
///    - Each message is automatically routed to its corresponding command handler <br/>
///    - Handlers process messages <b>asynchronously</b>, preventing connection blocking <br/>
///    - Perfect for handling chat messages, real-time updates, or any client-server communication <br/><br/>
/// <b>Note:</b> All message processing is fire-and-forget, ensuring non-blocking communication.
/// </summary>
/// <typeparam name="T">Your hub class that inherits from this base class.</typeparam>
public abstract class ManagedHub<T> : Hub<IClient> where T : Hub<IClient>
{
    protected readonly ILogger<ManagedHub<T>> _logger;
    protected readonly ManagedHubHelper<T> _hubHelper;
    private readonly HandlerBus _handlerBus;
    private readonly ManagedSignalRConfig _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedHub{T}"/> class.
    /// </summary>
    public ManagedHub
    (
        HandlerBus handlerBus,
        ILogger<ManagedHub<T>> logger,
        ManagedHubHelper<T> hubHelper,
        ManagedSignalRConfig configuration
    )
    {
        _handlerBus = handlerBus;
        _logger = logger;
        _hubHelper = hubHelper;
        _configuration = configuration;
    }

    /// <summary>
    /// Called when a connection with the hub is established. <br />
    /// <b>not to be overriden</b>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public sealed override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"[{typeof(T)}] [connected] {Context.ConnectionId}");
        await _hubHelper.AddConnectionAsync(Context);
        await OnConnectedHookAsync();
    }


    /// <summary>
    /// Contains operations to be executed after a connection is established. Can be overridden by derived classes.<br />
    /// </summary>
    protected virtual Task OnConnectedHookAsync() => Task.CompletedTask; 



    /// <summary>
    /// Called when a connection with the hub is disconnected.<br />
    /// <b>not to be overriden</b>
    /// </summary>
    /// <param name="exception">The exception that occurred during disconnection, if any.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public sealed override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"[{typeof(T)}] [disconnected] {Context.ConnectionId}");
        await _hubHelper.RemoveConnectionAsync(Context);

        await OnDisconnectedHookAsync();

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Contains operations to be executed after a disconnection occurs. Can be overridden by derived classes.
    /// </summary>
    protected virtual Task OnDisconnectedHookAsync() => Task.CompletedTask;



    /// <summary>
    /// Processes an incoming message by deserializing it and routing it to the appropriate
    /// <see cref="IManagedHubHandler{TCommand}"/> in a <b>fire &amp; forget </b> manner. <br/>
    /// Deserialization is driven by the configured topic-to-command mappings in <see cref="ManagedSignalRConfig"/>. <br/><br/>
    /// If overridden, the derived hub must manually handle deserialization and command handling.
    /// </summary>
    /// <param name="topic">The topic name identifying the command type to be dispatched.</param>
    /// <param name="message">The serialized JSON payload sent from the client.</param>
    public async Task NotifyServer(string topic, string message)
    {
        ManagedHubConfig? binding = _configuration.GetConfig(typeof(T));

        if (binding is null || !binding.Inbound.TryGetValue(topic, out var config))
            throw new InvalidOperationException($"No handler configured for topic {topic}");

        // Deserialize using configured deserializer
        object deserializedMessage = config.Deserializer(message);

        // Dispatch to handler
        await _handlerBus.Handle(deserializedMessage);
    }

}