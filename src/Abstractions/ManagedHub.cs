using ManagedLib.ManagedSignalR.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;

namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Base class for SignalR hubs with automatic message handling <br />
/// Features: <br />
/// - Automatic connection management <br />
/// - Automatic Message deserialization &amp; routing <br />
/// - Fire &amp; Forget async command processing <br />
/// </summary>
/// <remarks>
/// - Override <see cref="OnConnectedHookAsync"/> to execute custom logic when a client connects. <br/>
/// - Override <see cref="OnDisconnectedHookAsync"/> to execute custom logic when a client disconnects.
/// </remarks>
/// <typeparam name="T">Your hub class type</typeparam>
public abstract class ManagedHub<T> : Hub<IClient> where T : Hub<IClient>
{
    protected readonly ManagedHubHelper<T> _hubHelper;

    private readonly HandlerBus _handlerBus;
    private readonly ManagedSignalRConfig _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedHub{T}"/> class.
    /// </summary>
    public ManagedHub
    (
        HandlerBus handlerBus,
        ManagedHubHelper<T> hubHelper,
        ManagedSignalRConfig configuration
    )
    {
        _handlerBus = handlerBus;
        _hubHelper = hubHelper;
        _configuration = configuration;
    }

    /// <summary>
    /// Handles new client connections
    /// </summary>
    /// <remarks>- Override <see cref="OnConnectedHookAsync"/> to execute custom logic when a client connects. <br/></remarks>
    public sealed override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await _hubHelper.AddConnectionAsync(Context);
        await OnConnectedHookAsync();
    }

    /// <summary>
    /// Override to add custom logic on connection
    /// </summary>
    protected virtual Task OnConnectedHookAsync() => Task.CompletedTask;

    /// <summary>
    /// Handles client disconnections
    /// </summary>
    /// <remarks>- Override <see cref="OnDisconnectedHookAsync"/> to execute custom logic a client disconnects.</remarks>
    public sealed override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _hubHelper.RemoveConnectionAsync(Context);
        await base.OnDisconnectedAsync(exception);
        await OnDisconnectedHookAsync();
    }

    /// <summary>
    /// Override to add custom logic on disconnection
    /// </summary>
    protected virtual Task OnDisconnectedHookAsync() => Task.CompletedTask;



    /// <summary>
    /// Invoked by the client, the method processes incoming messages and routes them to handlers
    /// </summary>
    /// <param name="topic">Message topic for routing</param>
    /// <param name="message">Serialized message data</param>
    public async Task ReceiveFromClient(string topic, string message)
    {
        ManagedHubConfig? binding = _configuration.FindManagedHubConfig(typeof(T));

        if (binding is null || !binding.ReceiveConfig.TryGetValue(topic, out var config))
            throw new InvalidOperationException($"No handler configured for topic {topic}");

        // Deserialize using configured deserializer
        object deserializedMessage = config.Deserializer(message);

        // Dispatch to handler
        await _handlerBus.Handle(deserializedMessage, Context);
    }
}

/// <summary>
/// Base class for SignalR hubs that automatically infers the hub type
/// </summary>
public abstract class ManagedHub : ManagedHub<ManagedHub>
{
    protected ManagedHub(
        HandlerBus handlerBus,
        ManagedHubHelper<ManagedHub> hubHelper,
        ManagedSignalRConfig configuration
    ) : base(handlerBus, hubHelper, configuration)
    {
    }
}