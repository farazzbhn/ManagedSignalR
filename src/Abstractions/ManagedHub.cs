using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Abstractions;


public abstract class ManagedHub : Hub<IManagedHubClient>
{

    private readonly ManagedSignalRConfiguration _configuration;
    private readonly HubCommandDispatcher _dispatcher;
    private readonly ILogger<ManagedHub> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly IServiceProvider _serviceProvider;

    protected ManagedHub
    (
        ManagedSignalRConfiguration configuration,
        HubCommandDispatcher dispatcher,
        ILogger<ManagedHub> logger, 
        ICacheProvider cacheProvider, 
        IServiceProvider serviceProvider
    )
    {
        _configuration = configuration;
        _dispatcher = dispatcher;
        _logger = logger;
        _cacheProvider = cacheProvider;
        _serviceProvider = serviceProvider;
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

        // Try & associate the connection ID with the cached user session.
        try
        {
            // create the session and the respective cache entry  object
            var session = new ManagedHubSession(userId, connectionId, AppInfo.InstanceId);

            (string Key, string Value) entry = session.ToCacheKeyValue();

            if (_configuration.DeploymentMode == DeploymentMode.Distributed)
            {
                // cache the key/value pair cache using the default TTL.
                // The mechanism allows for automatic removal of instance-bound cache entries in case the app shuts down unexpectedly
                // A background service is then used to re-cache this entries before they fail
                await _cacheProvider.SetAsync(entry.Key, entry.Value, Constants.ManagedHubSessionCacheTtl);

                // caching the object locally allows for a background process to re-set the cache once it has been expired
                LocalCacheProvider<CacheEntry> localCache = _serviceProvider.GetRequiredService<LocalCacheProvider<CacheEntry>>();
                localCache.Set(new CacheEntry(entry.Key, entry.Value));
            }
            else // is a single instance application 
            {
                // values set within the instance-bound memory cache need not expire
                await _cacheProvider.SetAsync(entry.Key, entry.Value);

                // no need to cache keys using teh local cache provider. ( no background service required to re-cache expiring key/value pairs)
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

        // invoke the hook
        await OnDisconnectedHookAsync(userId);

        try
        {

            // try remove from the cache. 
            bool removed = await _cacheProvider.RemoveAsync(entry.Key);

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
            if (_configuration.DeploymentMode == DeploymentMode.Distributed)
            {
                LocalCacheProvider<CacheEntry> localCache = _serviceProvider.GetRequiredService<LocalCacheProvider<CacheEntry>>();
                bool removed = localCache.Remove(new CacheEntry(entry.Key, entry.Value));
            }
        }
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
    internal async Task InvokeServer(string topic, string message)
    {
        HubEndpointConfiguration configuration = _configuration.GetConfiguration(this.GetType());

        string userId = Context.UserIdentifier ?? Constants.Unauthenticated;

        // Deserialize using configured deserializer
        dynamic command = configuration.Deserialize(topic, message);

        // Dispatch to the registered handler
        await _dispatcher.Handle(command, Context, userId);
    }

}
