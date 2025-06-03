using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Helper;

/// <summary>
/// Thread-safe helper class for managing SignalR connection lifecycle and message routing using concurrent collections. <br/><br/>
/// <b>Core Components:</b> <br/><br/>
/// 1. <b>Connection State Management:</b> <br/>
///    - Uses ICacheProvider for thread-safe connection caching <br/>
///    - Maps userId to <see cref="ManagedHubConnection{T}"/> containing multiple connectionIds <br/>
///    - Handles connection state mutations via atomic operations <br/><br/>
/// 2. <b>Message Dispatch System:</b> <br/>
///    - Utilizes <see cref="IHubContext{THub, T}"/> for client communication <br/>
///    - Implements topic-based message routing via <see cref="EventMapping"/> configuration <br/>
///    - Supports payload serialization and type-safe message dispatch <br/><br/>
/// 3. <b>Implementation Details:</b> <br/>
///    - Async user identification via <see cref="IIdentityResolver"/> <br/>
///    - Connection lifecycle hooks: <see cref="AddConnectionAsync"/>, <see cref="RemoveConnectionAsync"/> <br/>
///    - Message routing via <see cref="TryWhisper"/> with automatic connection fan-out <br/><br/>
/// <b>Thread Safety:</b> <br/>
/// All public methods are thread-safe. Connection state modifications use ICacheProvider operations 
/// to ensure atomic updates in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">Hub type implementing <see cref="Hub{IClient}"/> for strongly-typed client invocations.</typeparam>
public class ManagedHubHelper<T> where T : Hub<IClient>
{
    protected readonly IHubContext<T, IClient> _hub;
    private readonly ManagedHubConfiguration _configuration;
    private readonly IIdentityResolver _idResolver;
    private readonly ILogger<ManagedHubHelper<T>> _logger;
    private readonly ICacheProvider _cacheProvider;

    public ManagedHubHelper
    (
        IHubContext<T, IClient> hub,
        ManagedHubConfiguration configuration,
        IIdentityResolver idResolver,
        ILogger<ManagedHubHelper<T>> logger,
        ICacheProvider cacheProvider
    )
    {
        _hub = hub;
        _configuration = configuration;
        _idResolver = idResolver;
        _logger = logger;
        _cacheProvider = cacheProvider;
    }

    public bool TryGetConnection(string userId, out ManagedHubConnection<T>? connection)
    {
        connection = _cacheProvider.Get<ManagedHubConnection<T>>(userId);
        return connection != null;
    }

    public async Task AddConnectionAsync(HubCallerContext context)
    {
        string userId = await _idResolver.GetUserId(context);

        var existingConnection = _cacheProvider.Get<ManagedHubConnection<T>>(userId);
        if (existingConnection == null)
        {
            existingConnection = new ManagedHubConnection<T>
            {
                UserId = userId,
                ConnectionIds = new ConcurrentBag<string>()
            };
        }

        // Add the new connection ID safely
        existingConnection.ConnectionIds.Add(context.ConnectionId);
        _cacheProvider.Set(userId, existingConnection);
    }

    public async Task RemoveConnectionAsync(HubCallerContext context)
    {
        string userId = await _idResolver.GetUserId(context);

        var connectionId = context.ConnectionId;
        var hubConnection = _cacheProvider.Get<ManagedHubConnection<T>>(userId);
        
        if (hubConnection != null)
        {
            if (hubConnection.ConnectionIds.Count > 1)
            {
                // Remove connection ID safely
                hubConnection.ConnectionIds.TryTake(out var removedId);
                if (removedId != null)
                {
                    // Update the cache
                    _cacheProvider.Set(userId, hubConnection);
                }
            }
            else
            {
                // No other connections, remove from cache
                _cacheProvider.Remove(userId);
            }
        }
        else
        {
            // WTH ?!
        }
    }

    /// <summary>
    /// 1. Finds the topic associated with the <paramref name="msg"/> ; ( registered at startup )<br />
    /// 2. Invokes the <see cref="TopicMessage."/><br />
    /// 3. Retrieves from within the cache provider, the list of connection ids associated with <paramref name="userId"></paramref> <br />
    /// 4. Invokes the client-side method <see cref="IClient.Whisper"/> using the two parameters for each connection Id<br />
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public async Task<bool> TryWhisper(string userId, TopicMessage msg)
    {
        try
        {
            var hubConnection = _cacheProvider.Get<ManagedHubConnection<T>>(userId);
            if (hubConnection != null)
            {
                var connectionIds = hubConnection.ConnectionIds;

                EventMapping binding = _configuration.GetMapping(typeof(T));
                string topic = binding.Outgoing.Single(x => x.Key == msg.GetType()).Value;
                string body = msg.ToText();
                // Send to each connection
                foreach (var id in connectionIds)
                {
                    await _hub.Clients.Client(id).Whisper(topic, body);
                }
                return true;
            }
            else
            {
                // userId may have disconnected 
                _logger.LogError($"[{GetType()}] Whisper failed : User {userId} has no active connections");
                return false;
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"[{GetType()}] Whisper failed : {e}");
            return false;
        }
    }
}

/// <summary>
/// Represents a userId-specific SignalR hub connection, including a collection of active connection IDs.
/// </summary>
public class ManagedHubConnection<T>
{
    public string UserId { get; set; } = default!;
    public ConcurrentBag<string> ConnectionIds { get; set; } = new();
}
