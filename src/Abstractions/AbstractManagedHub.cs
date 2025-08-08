using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Abstractions;

public abstract class AbstractManagedHub : Hub<IManagedHubClient>
{
    protected readonly IConnectionTracker Tracker;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubCommandDispatcher _dispatcher;

    internal AbstractManagedHub
    (
        ManagedSignalRConfiguration config,
        IServiceProvider serviceProvider,
        IHubCommandDispatcher dispatcher
    )
    {
        _serviceProvider = serviceProvider;
        _dispatcher = dispatcher;

        // retrieve the IConnectionTracker<ConcreteType> for the this implementation of managed hub
        Tracker = (IConnectionTracker)_serviceProvider.GetRequiredService(typeof(IConnectionTracker<>).MakeGenericType(GetType()));
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

        await Tracker.TrackAsync(Context);
  
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

        await Tracker.UntrackAsync(Context);

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
    /// <br/><br/>
    /// The method delegates to the injected <see cref="IHubCommandDispatcher"/> to handle the dispatching logic.
    /// </summary>
    /// <param name="topic">The message topic used for routing to the appropriate handler.</param>
    /// <param name="message">The serialized message payload.</param>
    /// <exception cref="ServiceNotRegisteredException">
    /// Thrown when no handler is registered for the resolved handler type.
    /// </exception>
    /// <exception cref="HandlerFailedException">
    /// Thrown when the handler's invocation throws an exception.
    /// </exception>
    public async Task InvokeServer(string topic, string message)
    {
        await _dispatcher.DispatchAsync(GetType(), topic, message, Context);
    }
}
