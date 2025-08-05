using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace ManagedLib.ManagedSignalR.Abstractions;

public abstract class ManagedHub : Hub<IManagedHubClient>
{
    private readonly ManagedSignalRConfiguration _config;
    private readonly ILogger<ManagedHub> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserConnectionManager _userConnectionManager;


    internal ManagedHub
    (
        ILogger<ManagedHub> logger,
        ManagedSignalRConfiguration config,
        IServiceProvider serviceProvider, 
        IUserConnectionManager userConnectionManager
    )
    {
        _logger = logger;
        _config = config;
        _serviceProvider = serviceProvider;
        _userConnectionManager = userConnectionManager;
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

        
        // add the connection to the list of connections associated with the user
        _userConnectionManager.AddConnection(this.GetType(), Context.UserIdentifier,  Context.ConnectionId, AppInfo.InstanceId);
        
        try
        {
            if (_config.DeploymentMode == DeploymentMode.Distributed)
            {
                // publish a message
            }
        }
        catch (Exception ex)
        {
            // Failed to cache the updated object => Log, abort, and clean up
            Context.Abort();

            _logger.LogError(
                "Failed to cache the new connection for user {UserId}:{ConnectionId}. Forcibly closed the connection. Exception: {ExceptionMessage}",
                Context.UserIdentifier, Context.ConnectionId, ex.Message);

            // remove the connection
            _userConnectionManager.RemoveConnection(this.GetType(), Context.UserIdentifier, Context.ConnectionId);

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

        string userId = Context.UserIdentifier ?? Constants.Anonymous;

        UserConnectionGroup connectionsGroup = new UserConnectionGroup(userId, Context.ConnectionId, AppInfo.InstanceId);
        KeyValuePair<string, string> entry = connectionsGroup.AsKeyValuePair();

        // Remove the session information from the in-memory cache.
        _memoryCache.Remove(entry.Key);

        try
        {
            if (_config.DeploymentMode == DeploymentMode.Distributed)
            {
                // TODO: Implement distributed cache synchronization logic here
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("OnDisconnectedAsync failed to run to completion. Exception: {ExceptionMessage}", ex.Message);
        }

        // Invoke the disconnection hook for custom logic
        await OnDisconnectedHookAsync();
    }

    /// <summary>
    /// Empty hook — Override to implement custom logic on connection. <br />
    /// Access the <see cref="Context"/> property to get connection details like <see cref="HubCallerContext.UserIdentifier"/> and <see cref="HubCallerContext.ConnectionId"/>.
    /// </summary>
    protected virtual Task OnConnectedHookAsync() => Task.CompletedTask;

    /// <summary>
    /// Empty hook — Override to implement custom logic on disconnection. <br />
    /// Access the <see cref="Context"/> property to get connection details like <see cref="HubCallerContext.UserIdentifier"/> and <see cref="HubCallerContext.ConnectionId"/>.
    /// </summary>
    protected virtual Task OnDisconnectedHookAsync() => Task.CompletedTask;

    /// <summary>
    /// Invoked by the client, processes incoming messages and routes them to handlers.
    /// </summary>
    /// <param name="topic">Message topic for routing.</param>
    /// <param name="message">Serialized message data.</param>
    /// <exception cref="ServiceNotRegisteredException">Thrown when the handler service is not registered.</exception>
    /// <exception cref="HandlerFailedException">Thrown when the handler invocation fails.</exception>
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
            await (Task)handleMethod.Invoke(handler, new object[] { command, Context });
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
