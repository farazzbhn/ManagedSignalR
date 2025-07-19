using System.Reflection.Metadata;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
namespace ManagedLib.ManagedSignalR.Abstractions;


public abstract class ManagedHub : Hub<IManagedHubClient>
{

    private readonly ManagedSignalRConfiguration _configuration;
    private readonly HubCommandDispatcher _dispatcher;
    private readonly ILogger<ManagedHub> _logger;
    private readonly IDistributedCacheProvider _distributedCacheProvider;
    private readonly LocalCacheProvider<CacheEntry> _localCacheProvider;
    protected ManagedHub
    (
        ManagedSignalRConfiguration configuration,
        HubCommandDispatcher dispatcher,
        ILogger<ManagedHub> logger, 
        IDistributedCacheProvider distributedCacheProvider, 
        LocalCacheProvider<CacheEntry> localCacheProvider
    )  
    {
        _configuration = configuration;
        _dispatcher = dispatcher;
        _logger = logger;
        _distributedCacheProvider = distributedCacheProvider;
        _localCacheProvider = localCacheProvider;
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

            await _distributedCacheProvider.SetAsync(entry.Key, entry.Value, Constants.SessionTTL);

            // caching the object locally allows for a background process to re-set the cache once it has been expired
            _localCacheProvider.Set(new CacheEntry(entry.Key, entry.Value));
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
        await OnConnectedHookAsync();
    }


    /// <summary>
    /// Handles client disconnections
    /// </summary>
    /// <remarks>- Override <see cref="OnDisconnectedHookAsync"/> to execute custom logic a client disconnects.</remarks>
    public sealed override async Task OnDisconnectedAsync(Exception? exception)
    {

        var userId = Context.UserIdentifier ?? Constants.Unauthenticated;
        var connectionId = Context.ConnectionId;

        (string Key, string Value) entry = new ManagedHubSession(userId, connectionId, AppInfo.InstanceId).ToCacheKeyValue();

        await OnDisconnectedHookAsync();
        try
        {
            bool removed_distributed = await _distributedCacheProvider.RemoveAsync(entry.Key);

            if (!removed_distributed)
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
            bool removed_local = _localCacheProvider.Remove(new CacheEntry(entry.Key, entry.Value));
        }
    }


    /// <summary>
    /// Empty hook -- Override to implement custom logic on connection
    /// </summary>
    protected virtual Task OnConnectedHookAsync() => Task.CompletedTask;

    ///<summary>
    /// Empty hook -- Override to implement custom logic on disconnection
    /// </summary>
    protected virtual Task OnDisconnectedHookAsync() => Task.CompletedTask;



    /// <summary>
    /// Invoked by the client, the method processes incoming messages and routes them to handlers
    /// </summary>
    /// <param name="topic">Message topic for routing</param>
    /// <param name="message">Serialized message data</param>
    internal async Task InvokeServer(string topic, string message)
    {
        HubEndpointConfiguration configuration = _configuration.GetConfiguration(this.GetType());

        // Deserialize using configured deserializer
        dynamic command = configuration.Deserialize(topic, message);

        // Dispatch to the registered handler
        await _dispatcher.Handle(command, Context);
    }

}

public class v
{
}
