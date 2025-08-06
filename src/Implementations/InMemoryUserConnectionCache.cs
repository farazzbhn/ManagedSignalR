using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Implementations;

internal class InMemoryUserConnectionCache : IUserConnectionCache
{

    private readonly IMemoryCache _cache;
    private readonly ManagedSignalRConfiguration _configuration;

    /// <summary>
    /// creates a unique cache key 
    /// </summary>
    /// <param name="hubType"></param>
    /// <param name="userIdentifier"></param>
    /// <returns></returns>
    private string KeyGen(Type hubType, string userIdentifier) =>
        $"{hubType.Name}:{_configuration.CachePrefix + userIdentifier.ToLowerInvariant()}";

    public InMemoryUserConnectionCache
    (
        IMemoryCache cache,
        ManagedSignalRConfiguration configuration
    )
    {
        _cache = cache;
        _configuration = configuration;
    }

    public void AddConnection(Type hubType, string userIdentifier, string connectionId, string instanceId)
    {
        if (hubType == null || !typeof(AbstractManagedHub).IsAssignableFrom(hubType))
            throw new InvalidOperationException(
                $"{hubType.Name} does not implement expected type {typeof(AbstractManagedHub)}");

        string key = KeyGen(hubType, userIdentifier);


        // Try to get the existing connection group for the user
        if (!_cache.TryGetValue(key, out UserConnectionGroup? group) || group is null)
        {
            // No existing group — create a new one with the passed instanceId
            group = new UserConnectionGroup(connectionId, instanceId);
        }
        else
        {
            // Add new connection to the existing group
            group.AddConnection(connectionId, instanceId);
        }

        // Update the cache with the new or updated group
        _cache.Set(key, group);
    }


    public void RemoveConnection(Type hubType, string userIdentifier, string connectionId)
    {

        if (hubType == null || !typeof(AbstractManagedHub).IsAssignableFrom(hubType))
            throw new InvalidOperationException(
                $"{hubType.Name} does not implement expected type {typeof(AbstractManagedHub)}");

        string key = KeyGen(hubType, userIdentifier);

        // Try to get the existing connection group from the cache
        if (!_cache.TryGetValue(key, out UserConnectionGroup? group) || group is null)
        {
            return; // Nothing to remove
        }

        // Remove the connection from the group
        group.RemoveConnection(connectionId);

        if (group.Connections.Count == 0)
        {
            // No more connections for this user — remove the whole group
            _cache.Remove(key);
        }
        else
        {
            // Update the group in the cache
            _cache.Set(key, group);
        }
    }


    public UserConnection[] GetUserConnections(Type hubType, string userIdentifier)
    {
        if (hubType == null || !typeof(AbstractManagedHub).IsAssignableFrom(hubType))
            throw new InvalidOperationException(
                $"{hubType.Name} does not implement expected type {typeof(AbstractManagedHub)}");

        string key = KeyGen(hubType, userIdentifier);

        // Try to get the existing connection group for the user
        if (_cache.TryGetValue(key, out UserConnectionGroup? group) && group is not null)
        {
            // Return the connections as an array
            return group.Connections.ToArray();
        }

        // No connections found for this user
        return [];
    }

}

