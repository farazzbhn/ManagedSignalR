using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR;

/// <summary>
/// Manages SignalR connections and message routing with thread safety. <br />
/// Features: <br />
/// - Thread-safe connection management implementing a distributed lock mechanism using <see cref="ICacheProvider"/><br />
/// - Topic-based message routing <br />
/// - Support for multiple connections per user <br />
/// - Automatic connection cleanup <br />
/// </summary>
/// <typeparam name="T">Hub type that implements EventBinding for client invocations</typeparam>
public class ManagedHubHelper<T> where T : Hub<IClient>
{

    protected readonly IHubContext<T, IClient> Hub;
    private readonly ManagedSignalRConfig _cfg;
    private readonly IIdentityProvider _identityProvider;
    private readonly ILogger<ManagedHubHelper<T>> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly DistributedLockProvider _lockProvider;


    public ManagedHubHelper
    (
        IHubContext<T, IClient> hub,
        ManagedSignalRConfig cfg,
        IIdentityProvider identityProvider,
        ILogger<ManagedHubHelper<T>> logger,
        ICacheProvider cacheProvider
    )
    {
        Hub = hub;
        _cfg = cfg;
        _identityProvider = identityProvider;
        _logger = logger;
        _cacheProvider = cacheProvider;
        _lockProvider = new DistributedLockProvider(_cacheProvider);
    }


    /// <summary>
    /// Adds a new connection to the user's session
    /// </summary>
    internal async Task<bool> TryAddConnectionAsync(HubCallerContext context)
    {
        string userId = _identityProvider.GetUserId(context);
        string? token = await _lockProvider.WaitAsync(userId);

        // failed to acquire lock => return false
        if (token is null)
        {
            _logger.LogError("Failed to acquire lock for user {UserId}", userId);
            return false;
        }

        try
        {
            var session = await _cacheProvider.GetAsync<ManagedHubSession>(userId);

            if (session == null)
            {
                session = new ManagedHubSession
                {
                    UserId = userId,
                    ConnectionIds = new List<string>()
                };
            }

            session.ConnectionIds.Add(context.ConnectionId);
            await _cacheProvider.SetAsync(userId, session);
            return true;
        }
        catch(Exception ex) 
        {
            _logger.LogError(ex, "Failed to add connection for user {UserId}", userId);
            return false;
        }
        finally
        {
            await _lockProvider.ReleaseAsync(userId, token);
        }

    }



    /// <summary>
    /// Removes a connection from the user's session and cleans up if no connections remain.
    /// </summary>
    /// <returns>True if the connection was removed or the session was deleted; false otherwise</returns>
    internal async Task<bool> TryRemoveConnectionAsync(HubCallerContext context)
    {
        string userId = _identityProvider.GetUserId(context);
        string connectionId = context.ConnectionId;

        string? token = await _lockProvider.WaitAsync(userId);
        if (token is null) return false;

        try
        {
            var session = await _cacheProvider.GetAsync<ManagedHubSession>(userId);

            if (session == null || !session.ConnectionIds.Any()) return false;

            bool removed = session.ConnectionIds.Remove(connectionId);

            if (!removed) return false;

            // remove the ManagedHubSession if user has no other active connections
            if (session.ConnectionIds.Count == 0)
            {
                await _cacheProvider.RemoveAsync(userId);
            }
            // or persist the updated ManagedHubSession if other active connections are found
            else
            {
                await _cacheProvider.SetAsync(userId, session);
            }

            return true;
        }
        finally
        {
            await _lockProvider.ReleaseAsync(userId, token);
        }
    }


    /// <summary>
    /// Sends a message to all connections of a specific user
    /// </summary>
    /// <typeparam name="TMessage">Type of message to send</typeparam>
    /// <param name="userId">Target user ID</param>
    /// <param name="message">Message to send</param>
    /// <returns>True if message was sent successfully</returns>
    public async Task<bool> TrySend<TMessage>(string userId, TMessage message)
    {
        try
        {
            ManagedHubSession? hubConnection = await _cacheProvider.GetAsync<ManagedHubSession>(userId);
            if (hubConnection == null || hubConnection.ConnectionIds.Count == 0)
            {
                _logger.LogWarning("[{Component}] Cannot send message to user '{UserId}' – no active connections",
                    nameof(ManagedHubHelper<T>), userId);
                return false;
            }

            ManagedHubConfig? binding = _cfg.GetManagedHubConfig(typeof(T));
            if (binding is null || !binding.SendConfig.TryGetValue(typeof(TMessage), out var config))
            {
                _logger.LogError("[{Component}] Send failed – No configuration found for message type {MessageType} on hub {HubType}",
                    nameof(ManagedHubHelper<T>), typeof(TMessage).FullName, typeof(T).Name);
                return false;
            }

            string serializedMessage = config.Serializer(message!);

            foreach (string connectionId in hubConnection.ConnectionIds)
            {
                await Hub.Clients.Client(connectionId).FireClient(config.Topic, serializedMessage);
                _logger.LogDebug("[{Component}] Sent message of type {MessageType} to connection {ConnectionId} (User: {UserId})",
                    nameof(ManagedHubHelper<T>), typeof(TMessage).Name, connectionId, userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Component}] Push failed for user '{UserId}' and message type {MessageType}",
                nameof(ManagedHubHelper<T>), userId, typeof(TMessage).Name);
            return false;
        }
    }

}

