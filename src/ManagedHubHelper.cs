using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Implementations;
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
/// <typeparam name="THub">Hub type that implements EventBinding for client invocations</typeparam>
public class ManagedHubHelper<THub> where THub : Hub<IManagedHubClient>
{

    protected IHubContext<THub, IManagedHubClient> Hub => _hub;

    private readonly IHubContext<THub, IManagedHubClient> _hub;
    private readonly GlobalSettings _configuration;
    private readonly ILogger<ManagedHubHelper<THub>> _logger;
    private readonly ICacheProvider _cacheProvider;
    private readonly DefaultLockProvider _lockProvider;


    public ManagedHubHelper
    (
        IHubContext<THub, IManagedHubClient> hub,
        GlobalSettings configuration,
        IIdentityProvider identityProvider,
        ILogger<ManagedHubHelper<THub>> logger,
        ICacheProvider cacheProvider
    )
    {
        _hub = hub;
        _configuration = configuration;
        _identityProvider = identityProvider;
        _logger = logger;
        _cacheProvider = cacheProvider;
        _lockProvider = new DefaultLockProvider(_cacheProvider);
    }


    /// <summary>
    /// Adds a new connection to the user's session ( within the cache )
    /// </summary>
    internal async Task<bool> TryAddConnectionAsync(HubCallerContext context)
    {
        string userId = _identityProvider.GetUserId(context);

        // try and acquire a lock 
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
        catch (Exception ex)
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
    /// Removes a connection from the user's session ( within the cache ) and cleans up if no connections remain.
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




}

