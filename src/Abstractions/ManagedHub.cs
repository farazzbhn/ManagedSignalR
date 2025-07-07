using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Types;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
namespace ManagedLib.ManagedSignalR.Abstractions;


public abstract class ManagedHub : Hub<IManagedHubClient>
{

    private readonly ManagedSignalRConfiguration _configuration;
    private readonly HubCommandDispatcher _dispatcher;
    private readonly ILogger<ManagedHub> _logger;
    private readonly IDistributedCacheProvider _cache;
    private readonly IDistributedLockProvider _lockProvider;

    protected ManagedHub
    (
        ManagedSignalRConfiguration configuration,
        HubCommandDispatcher dispatcher,
        ILogger<ManagedHub> logger, 
        IDistributedCacheProvider cache,
        IDistributedLockProvider lockProvider
    )
    {
        _configuration = configuration;
        _dispatcher = dispatcher;
        _logger = logger;
        _cache = cache;
        _lockProvider = lockProvider;
    }


    /// <summary>
    /// Handles new client connections
    /// </summary>
    /// <remarks>- Override <see cref="OnConnectedHookAsync"/> to execute custom logic when a client connects. <br/></remarks>
    public sealed override async Task OnConnectedAsync()
    {

        await base.OnConnectedAsync();

        string userId = Context.UserIdentifier ?? Constants.Anonymous;
        string connectionId = Context.ConnectionId;

        // Attempt to acquire a distributed ity lockProvider. 
        string? token = await _lockProvider.WaitAsync(userId);


        // Failed to acquire the distributed lockProvider => Abort & return
        if (token is null)      
        {
            _logger.LogError("Failed to acquire lock for {UserId}", userId);
            Context.Abort();
            return;
        }

        // Try & associate the connection ID with the cached user session.
        try
        {
            // retrieve the cached list of connections associated with the user
            var session = await _cache.GetAsync<ManagedHubSession>(userId);

            if (session == null)
            {
                session = new ManagedHubSession
                {
                    UserId = userId,
                    Connections = new ()
                };
            }

            var connection = new Connection(Constants.InstanceId, Context.ConnectionId);
            session.Connections.Add(connection);
            await _cache.SetAsync(userId, session);
        }
        // Failed to cache the updated object => Log, abort, and return.
        catch (Exception ex)    // Log & Abort
        {
            _logger.LogError(ex, "Failed to add/cache connection for user {UserId}", userId);
            Context.Abort();
            return;
        }

        // Release the distributed lockProvider
        finally
        {
            await _lockProvider.ReleaseAsync(userId, token);
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
        var userId = Context.UserIdentifier ?? Constants.Anonymous;
        var connectionId = Context.ConnectionId;

        await OnDisconnectedHookAsync();


        // Attempt to acquire a distributed ity lockProvider. 
        string? token = await _lockProvider.WaitAsync(userId);


        // Failed to acquire the distributed lockProvider => Abort & return
        if (token is null)
        {
            _logger.LogError("Failed to acquire lockProvider for {UserId}", userId);
            return;
        }
        
        try
        {
            var session = await _cache.GetAsync<ManagedHubSession>(userId);

            // connection id not found
            if (session == null || !session.Connections.Any())
            {
                _logger.LogError("Failed to remove/cache connection for user {UserId}", userId);
                return;
            }

            // remove the connection id 
            Connection? connection = session.Connections.FirstOrDefault(c => c.ConnectionId == connectionId);


            bool removed = connection is not null && session!.Connections.Remove(connection) ;

            // cannot remove (
            if (!removed)
            {
                _logger.LogError("Connection Id {ConnectionId} is not associated with user {UserId}. Failed to remove", userId, userId);
                return;
            }

            // check if the user has any other questions and if not proceed to de-cache the session
            if (session.Connections.Count == 0)
            {
                await _cache.RemoveAsync(userId);
                return;
            }

            // Update the ManagedHubSession if other active connections are found
            await _cache.SetAsync(userId, session);

        }
        finally
        {
            await _lockProvider.ReleaseAsync(userId, token);
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
