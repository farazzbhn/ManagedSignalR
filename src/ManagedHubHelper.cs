using System.Collections.Concurrent;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR;

/// <summary>
/// Thread-safe helper class for managing SignalR connection lifecycle and message routing using concurrent collections. <br/><br/>
/// <b>Core Components:</b> <br/><br/>
/// 1. <b>Connection State Management:</b> <br/>
///    - Uses <see cref="ICacheProvider"/> for thread-safe connection caching <br/>
///    - Maps userId to <see cref="ManagedHubConnection{T}"/> containing multiple connectionIds <br/>
///    - Handles connection state mutations via atomic operations <br/><br/>
/// 2. <b>Message Dispatch System:</b> <br/>
///    - Utilizes <see cref="IHubContext{THub, T}"/> for client communication <br/>
///    - Implements topic-based message routing via <see cref="IIdentityResolver"/> configuration <br/>
///    - Supports payload serialization and type-safe message dispatch <br/><br/>
/// 3. <b>Implementation Details:</b> <br/>
///    - Async user identification via <see cref="TryPush"/> <br/>
///    - Message routing via <see cref="AddConnectionAsync"/> with automatic connection fan-out <br/><br/>
///    - Connection lifecycle hooks:<br />
///      a) <see cref="RemoveConnectionAsync"/> <br />
///      b) <see cref="ICacheProvider"/> <br/><br/>
/// <b>Thread Safety:</b> <br/>
/// All public methods are thread-safe. <br />
/// Connection state modifications use <see cref="ICacheProvider"/> operations 
/// to ensure atomic updates in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">Hub type implementing <see cref="EventMapping"/> for strongly-typed client invocations.</typeparam>
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


    internal async Task AddConnectionAsync(HubCallerContext context)
    {
        string userId = await _idResolver.GetUserId(context);

        var existingConnection = _cacheProvider.Get<ManagedHubConnection<T>>(userId);
        if (existingConnection == null)
        {
            existingConnection = new ManagedHubConnection<T>
            {
                UserId = userId,
                ConnectionIds = new List<string>()
            };
        }

        // Add the new connection ID safely
        existingConnection.ConnectionIds.Add(context.ConnectionId);
        _cacheProvider.Set(userId, existingConnection);
    }

    internal async Task RemoveConnectionAsync(HubCallerContext context)
    {
        // find the user id associate with the context
        string userId = await _idResolver.GetUserId(context);

        // and connection Id
        string connectionId = context.ConnectionId;

        var hubConnection = _cacheProvider.Get<ManagedHubConnection<T>>(userId);
        
        if (hubConnection != null)
        {
            if (hubConnection.ConnectionIds.Count > 1)
            {
                // Remove connection ID 
                hubConnection.ConnectionIds.Remove(connectionId);
            }
            else
            {
                // No other connections, remove from cache
                _cacheProvider.Remove(userId);
            }
        }
        else // object reference not expired
        {
            _logger.LogWarning($"{_cacheProvider.GetType()} Failed to find the cache, is it expired?");
        }
    }

    /// <summary>
    /// 1. Finds the topic associated with the <paramref name="msg"/> ; ( registered at startup )<br />
    /// 2. Invokes the <see cref="IPushNotiIPushNotification.ToPayload
    /// 3. Retrieves from within the cache provider, the list of connection ids associated with <paramref name="userId"></paramref> <br />
    /// 4. Invokes the client-side method <see cref="IClient.Push"/> using the two parameters for each connection Id<br />
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    public async Task<bool> TryPush(string userId, IPushNotification msg)
    {
        try
        {
            var hubConnection = _cacheProvider.Get<ManagedHubConnection<T>>(userId);
            if (hubConnection != null)
            {
                var connectionIds = hubConnection.ConnectionIds;

                EventMapping binding = _configuration.GetMapping(typeof(T));
                string topic = binding.Outgoing.Single(x => x.Key == msg.GetType()).Value;
                string payload = msg.ToPayload();
                // Send to each connection
                foreach (var id in connectionIds)
                {
                    await _hub.Clients.Client(id).Push(topic, payload);
                }
                return true;
            }
            else
            {
                // userId may have disconnected 
                _logger.LogError($"[{GetType()}] Push failed : User {userId} has no active connections");
                return false;
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"[{GetType()}] Push failed : {e}");
            return false;
        }
    }
}

