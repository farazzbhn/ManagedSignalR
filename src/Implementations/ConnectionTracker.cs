
using System.Collections.Concurrent;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Implementations;

internal class ConnectionTracker<THub> : IConnectionTracker<THub> where THub : AbstractManagedHub
{

    private readonly ConcurrentDictionary<string, ConnectionSet> _sets = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();


    public async Task TrackAsync(HubCallerContext context)
    {
        string connectionId = context.ConnectionId;
        string key = context.UserIdentifier ?? Constants.Unauthenticated;

        SemaphoreSlim sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            if (!_sets.TryGetValue(key, out ConnectionSet? set) || set is null)
            {
                // GetTracker new group for this user
                set = new ConnectionSet(connectionId);
                _sets[key] = set;
            }
            else
            {
                // Add connection to existing group
                set.AddConnection(connectionId);
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
            if (_sets.TryGetValue(key, out ConnectionSet? set) && set is not null)
            {
                bool removed = set.RemoveConnection(connectionId);
                
                // If no more connections for this user, remove the group and clean up the lock
                if (set.Connections.Count == 0)
                {
                    _sets.TryRemove(key, out _);
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

