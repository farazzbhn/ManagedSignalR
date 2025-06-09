using System.Collections.Concurrent;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR;

/// <summary>
/// Manages SignalR connections and message routing with thread safety. <br />
/// Features: <br />
/// - Thread-safe connection management using <see cref="ICacheProvider "/><br />
/// - Topic-based message routing <br />
/// - Support for multiple connections per user <br />
/// - Automatic connection cleanup <br />
/// </summary>
/// <typeparam name="T">Hub type that implements EventBinding for client invocations</typeparam>
public class ManagedHubHelper<T> where T : Hub<IClient>
{
    protected readonly IHubContext<T, IClient> _hub;

    private readonly ManagedSignalRConfig _configuration;
    private readonly IUserIdResolver _idResolver;
    private readonly ILogger<ManagedHubHelper<T>> _logger;
    private readonly ICacheProvider _cacheProvider;


    public ManagedHubHelper
    (
        IHubContext<T, IClient> hub,
        ManagedSignalRConfig configuration,
        IUserIdResolver idResolver,
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


    /// <summary>
    /// Adds a new connection to the user's session
    /// </summary>
    internal async Task AddConnectionAsync(HubCallerContext context)
    {
        string userId =  _idResolver.GetUserId(context);

        var session = await _cacheProvider.GetAsync<ManagedHubSession<T>>(userId);

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
        await _cacheProvider.SetAsync(userId, session);
    }

    /// <summary>
    /// Removes a connection from the user's session and cleans up if no connections remain
    /// </summary>
    internal async Task RemoveConnectionAsync(HubCallerContext context)
    {
        // find the user id associate with the context
        string userId = _idResolver.GetUserId(context);

        // and connection Id
        string connectionId = context.ConnectionId;

        var session = await _cacheProvider.GetAsync<ManagedHubSession<T>>(userId);
        
        if (session != null)
        {
            if (session.ConnectionIds.Count > 1)
            {
                // RemoveAsync connection ID 
                session.ConnectionIds.Remove(connectionId);
            }
            else
            {
                // No other connections, remove from cache
                await _cacheProvider.RemoveAsync(userId);
            }
        }
        else // object reference not expired
        {
            _logger.LogWarning($"{_cacheProvider.GetType()} Failed to find the cache, is it expired?");
        }
    }

    /// <summary>
    /// Sends a message to all connections of a specific user
    /// </summary>
    /// <typeparam name="TMessage">Type of message to send</typeparam>
    /// <param name="userId">Target user ID</param>
    /// <param name="message">Message to send</param>
    /// <returns>True if message was sent successfully</returns>
    public async Task<bool> TrySendToClient<TMessage>(string userId, TMessage message)
    {
        try
        {
            ManagedHubSession<T>? hubConnection = await _cacheProvider.GetAsync<ManagedHubSession<T>>(userId);
            if (hubConnection != null)
            {
                var connectionIds = hubConnection.ConnectionIds;
                ManagedHubConfig? binding = _configuration.FindManagedHubConfig(typeof(T));

                if (binding is null || !binding.SendConfig.TryGetValue(typeof(TMessage), out var config))
                {
                    _logger.LogError($"[{GetType()}] Push failed: No configuration found for message type {typeof(TMessage)}");
                    return false;
                }

                string serializedMessage = config.Serializer(message!);

                // Send to each connection
                foreach (string id in connectionIds)
                {
                    await _hub.Clients.Client(id).SendToClient(config.Topic, serializedMessage);
                }
                return true;
            }
            else
            {
                // userId may have disconnected 
                _logger.LogError($"[{GetType()}] SendToClient failed : User {userId} has no active connections");
                return false;
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"[{GetType()}] SendToClient failed : {e}");
            return false;
        }
    }
}

