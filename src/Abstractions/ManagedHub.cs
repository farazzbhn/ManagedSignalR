using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Abstractions;


public abstract class ManagedHub : Hub<IManagedHubClient>
{

    private readonly ManagedSignalRConfiguration _globalConfiguration;
    private readonly ILogger<ManagedHub> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly LocalCacheProvider<ManagedHubSession> _localCache;
    internal ManagedHub
    (
        ManagedSignalRConfiguration globalConfiguration,
        ILogger<ManagedHub> logger, 
        IDistributedCacheProvider cacheProvider, 
        IServiceProvider serviceProvider, 
        LocalCacheProvider<ManagedHubSession> localCache
    )
    {
        _logger = logger;
        _globalConfiguration = globalConfiguration;
        _serviceProvider = serviceProvider;
        _localCache = localCache;
    }

    /// <summary>
    /// Handles new client connections
    /// </summary>
    /// <remarks>- Override <see cref="OnConnectedHookAsync"/> to execute custom logic when a client connects. <br/></remarks>
    public sealed override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        string userId = Context.UserIdentifier ?? Constants.Unauthenticated;
        string connectionId = Context.ConnectionId;

        // create the session and the respective cache entry  object
        // Try & associate the connection ID with the cached user session.
        var session = new ManagedHubSession(userId, connectionId, AppInfo.InstanceId);

        // store the session information locally to be accessed later
        _localCache.Add(session);

        try
        {
            if (_globalConfiguration.DeploymentMode == DeploymentMode.Distributed)
            {
                // create the key value set for the sesion
                (string Key, string Value) entry = session.ToCacheKeyValue();

                IDistributedCacheProvider distributedCache = _serviceProvider.GetRequiredService<IDistributedCacheProvider>();

                // cache the key/value pair cache using the default TTL.
                // The mechanism allows for automatic removal of instance-bound cache entries in case the app shuts down unexpectedly
                // A background service is then used to re-cache this entries before they fail
                await distributedCache.SetAsync(entry.Key, entry.Value, Constants.ManagedHubSessionCacheTtl);
            }

        }
        catch (Exception ex) // Failed to cache the updated object => Log, abort, and return.
        {
            Context.Abort();

            _logger.LogError(message:"Failed to cache the new connection for user {UserId}:{ConnectionId}.\n" +
                                     "Forcibly closed the connection.\n" +
                                     "Exception {ex}", userId, connectionId, ex.Message
            );

            return;
        }

        // Invoke the hook
        await OnConnectedHookAsync(userId);
    }


    /// <summary>
    /// Handles client disconnections
    /// </summary>
    /// <remarks>- Override <see cref="OnDisconnectedHookAsync"/> to execute custom logic a client disconnects.</remarks>
    public sealed override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        string userId = Context.UserIdentifier ?? Constants.Unauthenticated;
        string? connectionId = Context.ConnectionId;

        (string Key, string Value) entry = new ManagedHubSession(userId, connectionId, AppInfo.InstanceId).ToCacheKeyValue();


        bool removed = await _cacheProvider.RemoveAsync(entry.Key);

        try
        {

            // try remove from the cache. 

            if (!removed) 
            {
                _logger.LogWarning($"Cache entry {entry.Key} cannot be deleted.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(message: $"OnDisconnectedAsync failed to run to completion.\n" +
                                        $"{ex.Message}"
            );
        }

        finally
        {
            // remove the connection from the instance-bound memory cache which is used by the background service to re-cache the expiring key/value pairs
            if (_globalConfiguration.DeploymentMode == DeploymentMode.Distributed)
            {
                LocalCacheProvider<ManagedHubSessionCacheEntry> localCache = _serviceProvider.GetRequiredService<LocalCacheProvider<ManagedHubSessionCacheEntry>>();
                bool removed = localCache.Remove(new ManagedHubSessionCacheEntry(entry.Key, entry.Value));
            }
            // nothing else to remove for Single-instance connections
        }

        // invoke the hook
        await OnDisconnectedHookAsync(userId);
    }

    /// <summary>
    /// Empty hook -- Override to implement custom logic on connection
    /// </summary>
    protected virtual Task OnConnectedHookAsync(string userId) => Task.CompletedTask;

    ///<summary>
    /// Empty hook -- Override to implement custom logic on disconnection
    /// </summary>
    protected virtual Task OnDisconnectedHookAsync(string userId) => Task.CompletedTask;


    /// <summary>
    /// Invoked by the client, the method processes incoming messages and routes them to handlers
    /// </summary>
    /// <param name="topic">Message topic for routing</param>
    /// <param name="message">Serialized message data</param>
    public async Task InvokeServer(string topic, string message)
    {
        HubEndpointOptions options = _globalConfiguration.GetHubEndpointOptions(this.GetType());

        string userId = Context.UserIdentifier ?? Constants.Unauthenticated;

        // Deserialize using configured deserializer
        dynamic command = options.Deserialize(topic, message);

        // retrieve the specified handler type from the configuration
        Type handlerType = options.GetHandlerType(topic);

        // and get from the service provider
        // handler is an implementation of IHubCommandHandler<>
        object? handler = _serviceProvider.GetService(handlerType);

        if (handler == null) throw new ServiceNotRegisteredException(handlerType.ToString());

        var handleAsyncMethod = handlerType.GetMethod("Handle");

        try
        {
            await (Task)handleAsyncMethod.Invoke(handler, new object[] { command, Context, userId });
        }
        catch (Exception exception)
        {
            throw new HandlerFailedException(handlerType, exception);
        }
    }
}
