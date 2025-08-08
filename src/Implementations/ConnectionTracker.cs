
using System.Collections.Concurrent;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Implementations;

internal class ConnectionTracker<THub> : IConnectionTracker<THub> where THub : AbstractManagedHub
{

    private readonly ConcurrentDictionary<string, ConnectionGroup> _groups = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();



    public async Task TrackAsync(HubCallerContext context)
    {
        string connectionId = context.ConnectionId;
        string key = context.UserIdentifier ?? Constants.Unauthenticated;

        SemaphoreSlim sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            if (!_groups.TryGetValue(key, out ConnectionGroup? group) || group is null)
            {
                // Create new group for this user
                group = new ConnectionGroup(connectionId);
                _groups[key] = group;
            }
            else
            {
                // Add connection to existing group
                group.AddConnection(connectionId);
            }
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task UntrackAsync(HubCallerContext context)
    {
        string connectionId = context.ConnectionId;
        string key = context.UserIdentifier ?? Constants.Unauthenticated;

        SemaphoreSlim sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            if (_groups.TryGetValue(key, out ConnectionGroup? group) && group is not null)
            {
                bool removed = group.RemoveConnection(connectionId);
                
                // If no more connections for this user, remove the group and clean up the lock
                if (group.Connections.Count == 0)
                {
                    _groups.TryRemove(key, out _);
                    _locks.TryRemove(key, out var lockToDispose);
                    lockToDispose?.Dispose();
                }
            }
        }
        finally
        {
            sem.Release();
        }
    }
}

