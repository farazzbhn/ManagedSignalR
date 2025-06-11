using ManagedLib.ManagedSignalR.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ManagedLib.ManagedSignalR.Exceptions;
using ManagedLib.ManagedSignalR.Implementations;

namespace ManagedLib.ManagedSignalR.Abstractions;


public abstract class ManagedHub : Hub<IManagedHubClient>
{

    private readonly GlobalSettings _settings;
    private readonly ManagedHubHandlerBus _bus;
    private readonly ILogger<ManagedHub> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILockProvider _lockProvider;

    protected ManagedHub
    (
        GlobalSettings settings,
        ManagedHubHandlerBus bus,
        ILogger<ManagedHub> logger, 
        ICacheProvider cacheProvider,
        ILockProvider lockProvider
    )
    {
        _settings = settings;
        _bus = bus;
        _logger = logger;
        _cacheProvider = cacheProvider;
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

        // Attempt to acquire a distributed ity lock. 
        string? token = await _lockProvider.WaitAsync(userId);


        // Failed to acquire the distributed lock => Abort & return
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
            var session = await _cacheProvider.GetAsync<ManagedHubSession>(userId);

            if (session == null)
            {
                session = new ManagedHubSession
                {
                    UserId = userId,
                    ConnectionIds = new List<string>()
                };
            }

            session.ConnectionIds.Add(Context.ConnectionId);
            await _cacheProvider.SetAsync(userId, session);
        }
        // Failed to cache the updated object => Log, abort, and return.
        catch (Exception ex)    // Log & Abort
        {
            _logger.LogError(ex, "Failed to add/cache connection for user {UserId}", userId);
            Context.Abort();
            return;
        }

        // Release the distributed lock
        finally
        {
            await _lockProvider.ReleaseAsync(userId, token);
        }

        // Invoke the hook
        await OnConnectedHookAsync(userId, connectionId);
    }


    /// <summary>
    /// Handles client disconnections
    /// </summary>
    /// <remarks>- Override <see cref="OnDisconnectedHookAsync"/> to execute custom logic a client disconnects.</remarks>
    public sealed override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier ?? Constants.Anonymous;
        var connectionId = Context.ConnectionId;

        await OnDisconnectedHookAsync(userId, connectionId);


        // Attempt to acquire a distributed ity lock. 
        string? token = await _lockProvider.WaitAsync(userId);


        // Failed to acquire the distributed lock => Abort & return
        if (token is null)
        {
            _logger.LogError("Failed to acquire lock for {UserId}", userId);
            return;
        }
        
        try
        {
            var session = await _cacheProvider.GetAsync<ManagedHubSession>(userId);

            // connection id not found
            if (session == null || !session.ConnectionIds.Any())
            {
                _logger.LogError("Failed to remove/cache connection for user {UserId}", userId);
                return;
            }

            // remove the connection id 
            bool removed = session!.ConnectionIds.Remove(connectionId);

            // cannot remove (
            if (!removed)
            {
                _logger.LogError("Connection Id {ConnectionId} is not associated with user {UserId}. Failed to remove", userId, userId);
                return;
            }

            // check if the user has any other questions and if not proceed to de-cache the session
            if (session.ConnectionIds.Count == 0)
            {
                await _cacheProvider.RemoveAsync(userId);
                return;
            }

            // Update the ManagedHubSession if other active connections are found
            await _cacheProvider.SetAsync(userId, session);

        }
        finally
        {
            await _lockProvider.ReleaseAsync(userId, token);
        }

    }

    /// <summary>
    /// Invoked by the client, the method processes incoming messages and routes them to handlers
    /// </summary>
    /// <param name="topic">Message topic for routing</param>
    /// <param name="message">Serialized message data</param>
    internal async Task InvokeServer(string topic, string message)
    {
        ManagedHubConfiguration? configuration = _settings.FindConfiguration(this.GetType());

        if (configuration is null || !configuration.ReceiveConfig.TryGetValue(topic, out var bindings))
            throw new InvalidOperationException($"No handler configured for topic {topic}");

        // Deserialize using configured deserializer
        var deserializedMessage = bindings.Deserializer(message);

        // Dispatch to the registered handler
        await _bus.Handle(deserializedMessage, Context);
    }

    /// <summary>
    /// Empty hook &amp; Override to add custom logic on connection
    /// </summary>
    protected virtual Task OnConnectedHookAsync(string userId, string connectionId) => Task.CompletedTask;

    /// Empty hook &amp; Override to add custom logic on disconnection
    /// </summary>
    protected virtual Task OnDisconnectedHookAsync(string userId, string connectionId) => Task.CompletedTask;

}
