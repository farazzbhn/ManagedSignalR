using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;

public abstract class AbstractManagedHub : Hub<IManagedHubClient>
{
    private readonly IConnectionTracker _connections;
    private readonly IHubCommandDispatcher _dispatcher;

    internal AbstractManagedHub
    (
        ManagedSignalRConfiguration config,
        IConnectionTrackerFactory connectionTrackerFactory,
        IHubCommandDispatcher dispatcher
    )
    {
        _dispatcher = dispatcher;
        _connections = connectionTrackerFactory.GetTracker(GetType());
    }

    /// <summary>
    /// Handles new client connections.
    /// </summary>
    /// <remarks>
    /// Override <see cref="OnConnectedHookAsync"/> to run custom logic when a client connects.
    /// </remarks>
    public sealed override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        await _connections.TrackAsync(Context);
  
        // Invoke the connection hook for custom logic
        await OnConnectedHookAsync();
    }

    /// <summary>
    /// Handles client disconnections.
    /// </summary>
    /// <remarks>
    /// Override <see cref="OnDisconnectedHookAsync"/> to execute custom logic when a client disconnects.
    /// </remarks>
    public sealed override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        await _connections.UntrackAsync(Context);

        // Invoke the disconnection hook for custom disconnection logic
        await OnDisconnectedHookAsync();
    }

    /// <summary>
    /// Empty hook — Override to implement custom logic on connection. <br />
    /// Access the <see cref="Hub.Context"/> property to get connection details like <see cref="HubCallerContext.UserIdentifier"/> and <see cref="HubCallerContext.ConnectionId"/>.
    /// </summary>
    protected virtual Task OnConnectedHookAsync() => Task.CompletedTask;

    /// <summary>
    /// Empty hook — Override to implement custom logic on disconnection. <br />
    /// Access the <see cref="Hub.Context"/> property to get connection details like <see cref="HubCallerContext.UserIdentifier"/> and <see cref="HubCallerContext.ConnectionId"/>.
    /// </summary>
    protected virtual Task OnDisconnectedHookAsync() => Task.CompletedTask;

    /// <summary>
    /// Invoked by the client to process a message routed by topic.
    /// </summary>
    /// <param name="topic">The message topic used for routing to the appropriate handler.</param>
    /// <param name="message">The serialized message payload.</param>
    /// <exception cref="ServiceNotRegisteredException">
    /// Thrown when no handler is registered for the resolved handler type.
    /// </exception>
    /// <exception cref="HandlerFailedException">
    /// Thrown when the handler's invocation throws an exception.
    /// </exception>
    public Task InvokeServer(string topic, string message) => _dispatcher.FireAndForget(GetType(), topic, message, Context);
}
