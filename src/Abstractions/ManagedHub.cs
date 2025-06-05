using ManagedLib.ManagedSignalR.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// A ready-to-use SignalR hub base class that simplifies real-time communication between clients and server. <br/><br/>
/// <b>How to use:</b> <br/><br/>
/// 1. <b>Connection Management:</b> <br/>
///    - Inherit from this class to automatically handle client connections and disconnections <br/>
///    - Override <see cref="OnConnectedHookAsync"/> to run custom logic when clients connect <br/>
///    - Override <see cref="OnDisconnectedHookAsync"/> to clean up resources when clients disconnect <br/><br/>
/// 2. <b>Message Handling:</b> <br/>
///    - Clients send messages using the <b><see cref="Submit"/></b> method with a topic and JSON payload <br/>
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
    private readonly HandlerBus postBus;
    private readonly ManagedHubConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedHub{T}"/> class.
    /// </summary>
    public ManagedHub
    (
        HandlerBus postBus,
        ILogger<ManagedHub<T>> logger,
        ManagedHubHelper<T> hubHelper,
        ManagedHubConfiguration configuration
    )
    {
        this.postBus = postBus;
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
    protected virtual Task OnConnectedHookAsync() { return Task.CompletedTask; }

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
    protected virtual Task OnDisconnectedHookAsync() { return Task.CompletedTask; }


    /// <summary>
    /// Processes an incoming message by deserializing it and dispatching it via <see cref="HandlerBus"/> in a fire-and-forget manner. <br/>
    /// Deserialization is driven by the configured topic-to-command mappings in <see cref="ManagedHubConfiguration"/>. <br/><br/>
    /// If overridden, the derived hub must manually handle deserialization and command dispatching.
    /// </summary>
    /// <param name="topic">The topic name identifying the command type to be dispatched.</param>
    /// <param name="message">The serialized JSON payload sent from the client.</param>
    public async Task Submit(string topic, string message)
    {
        // Determine the target type based on the topic
        EventMapping binding = _configuration.GetMapping(typeof(T));

        Type targetType = binding.Incoming.Single(x => x.Value == topic).Key;

        // Deserialize the body into an object of the target type
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        dynamic deserializedBody = JsonSerializer.Deserialize(message, targetType, options)!;

        // send the deserialized body to the handled
        await postBus.HandleAsync(deserializedBody);
    }

}