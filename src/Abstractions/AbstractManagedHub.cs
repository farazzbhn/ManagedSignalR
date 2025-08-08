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
    private readonly ManagedSignalRConfiguration _config;
    private readonly ILogger<AbstractManagedHub> _logger;
    private readonly IConnectionTracker _tracker;
    private readonly IServiceProvider _serviceProvider;

    internal AbstractManagedHub
    (
        ILogger<AbstractManagedHub> logger,
        ManagedSignalRConfiguration config,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _config = config;
        _serviceProvider = serviceProvider;

        // retrieve the IConnectionTracker<ConcreteType> for the this implementation of managed hub
        _tracker = (IConnectionTracker)_serviceProvider.GetRequiredService(typeof(IConnectionTracker<>).MakeGenericType(GetType()));
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


        await _tracker.TrackAsync(Context);
        
        try
        {
            if (_config.DeploymentMode == DeploymentMode.Distributed)
            {
                // publish a message
            }
        }
        catch (Exception ex)
        {
            // untrack the connection
            await _tracker.UntrackAsync(Context);

            Context.Abort();

            _logger.LogError($"Failed to publish connection event:\t{ex.Message}");

            return;
        }

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

        await _tracker.UntrackAsync(Context);

        try
        {
            if (_config.DeploymentMode == DeploymentMode.Distributed)
            {
                // publish a disconnected event
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to publish disconnection event:\t{ex.Message}");
        }

        // Invoke the disconnection hook for custom logic
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
    /// The method performs the following steps:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Determines the corresponding C# type  for the payload based on the combination 
    ///       of the <b>current hub type</b> and <b>topic</b>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Deserializes the message using the <b>pre-configured deserializer</b> method.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Resolves the corresponding <see cref="IHubCommandHandler{TCommand}"/> from the service provider.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Invokes the handler with the deserialized command and the current Hub context.
    ///     </description>
    ///   </item>
    /// </list>
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
        // Retrieve the options configured for this hub endpoint
        HubEndpointOptions hubEndpointOptions = _config.GetHubEndpointOptions(this.GetType());

        // Deserialize the payload into a specific c# type based on topic and message
        dynamic command = hubEndpointOptions.Deserialize(topic, message);

        // Retrieve the handler type for the topic as registered configuration. 
        // i.e, IHubCommandHandler<Command> where Command is the type of the command being handled.
        Type handlerType = hubEndpointOptions.GetHandlerType(topic);

        // Get the handler instance from the service provider
        object? handler = _serviceProvider.GetService(handlerType);

        if (handler == null) throw new ServiceNotRegisteredException(handlerType.ToString());

        MethodInfo? handleMethod = handlerType.GetMethod("Handle");

        if (handleMethod == null) throw new MissingMethodException($"Handle method not found on handler type {handlerType}");

        try
        {
            await (Task)handleMethod!.Invoke(handler, [command, Context])!;
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            throw new HandlerFailedException(handlerType, tie.InnerException);
        }
        catch (Exception ex)
        {
            throw new HandlerFailedException(handlerType, ex);
        }
    }
}
