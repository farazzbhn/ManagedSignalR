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
///    - Maps userId to <see cref="ManagedHubSession{T}"/> containing multiple connectionIds <br/>
///    - Handles connection state mutations via atomic operations <br/><br/>
/// 2. <b>Message Dispatch System:</b> <br/>
///    - Utilizes <see cref="IHubContext{THub, T}"/> for client communication <br/>
///    - Implements topic-based message routing via <see cref="IIdentityResolver"/> configuration <br/>
///    - Supports payload serialization and type-safe message dispatch <br/><br/>
/// 3. <b>Implementation Details:</b> <br/>
///    - Async user identification via <see cref="TryPush{TMessage}"/> <br/>
///    - Message routing via <see cref="AddConnectionAsync"/> with automatic connection fan-out <br/><br/>
///    - Connection lifecycle hooks:<br />
///      a) <see cref="RemoveConnectionAsync"/> <br />
///      b) <see cref="ICacheProvider"/> <br/><br/>
/// <b>Thread Safety:</b> <br/>
/// All public methods are thread-safe. <br />
/// Connection state modifications use <see cref="ICacheProvider"/> operations 
/// to ensure atomic updates in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">Hub type implementing <see cref="EventBinding"/> for strongly-typed client invocations.</typeparam>
public class ManagedHubHelper<T> where T : Hub<IClient>
{
    protected readonly IHubContext<T, IClient> _hub;

    private readonly ManagedSignalRConfig _configuration;
    private readonly IIdentityResolver _idResolver;
    private readonly ILogger<ManagedHubHelper<T>> _logger;
    private readonly Func<ICacheProvider> _cacheProviderFactory;


    public ManagedHubHelper
    (
        IHubContext<T, IClient> hub,
        ManagedSignalRConfig configuration,
        IIdentityResolver idResolver,
        ILogger<ManagedHubHelper<T>> logger,
        Func<ICacheProvider> cacheProviderFactory
    )
    {
        _hub = hub;
        _configuration = configuration;
        _idResolver = idResolver;
        _logger = logger;
        _cacheProviderFactory = cacheProviderFactory;
    }


    internal async Task AddConnectionAsync(HubCallerContext context)
    {
        string userId = await _idResolver.GetUserId(context);

        var session = _cacheProviderFactory().Get<ManagedHubSession<T>>(userId);
        if (session == null)
        {
            session = new ManagedHubSession<T>
            {
                UserId = userId,
                ConnectionIds = new List<string>()
            };
        }

        // Add the new connection ID safely
        session.ConnectionIds.Add(context.ConnectionId);
        _cacheProviderFactory().Set(userId, session);
    }

    internal async Task RemoveConnectionAsync(HubCallerContext context)
    {
        // find the user id associate with the context
        string userId = await _idResolver.GetUserId(context);

        // and connection Id
        string connectionId = context.ConnectionId;

        var session = _cacheProviderFactory().Get<ManagedHubSession<T>>(userId);
        
        if (session != null)
        {
            if (session.ConnectionIds.Count > 1)
            {
                // Remove connection ID 
                session.ConnectionIds.Remove(connectionId);
            }
            else
            {
                // No other connections, remove from cache
                _cacheProviderFactory().Remove(userId);
            }
        }
        else // object reference not expired
        {
            _logger.LogWarning($"{_cacheProviderFactory().GetType()} Failed to find the cache, is it expired?");
        }
    }

    /// <summary>
    /// Pushes a message to all connections of a specific user.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to push</typeparam>
    /// <param name="userId">The user ID to push the message to</param>
    /// <param name="message">The message to push</param>
    /// <returns>True if the message was pushed successfully, false otherwise</returns>
    public async Task<bool> TryPush<TMessage>(string userId, TMessage message)
    {
        try
        {
            ManagedHubSession<T>? hubConnection = _cacheProviderFactory().Get<ManagedHubSession<T>>(userId);
            if (hubConnection != null)
            {
                var connectionIds = hubConnection.ConnectionIds;
                ManagedHubConfig? binding = _configuration.GetConfig(typeof(T));

                if (binding is null || !binding.Outbound.TryGetValue(typeof(TMessage), out var config))
                {
                    _logger.LogError($"[{GetType()}] Push failed: No configuration found for message type {typeof(TMessage)}");
                    return false;
                }

                string serializedMessage = config.Serializer(message!);

                // Send to each connection
                foreach (string id in connectionIds)
                {
                    await _hub.Clients.Client(id).Push(config.Topic, serializedMessage);
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

