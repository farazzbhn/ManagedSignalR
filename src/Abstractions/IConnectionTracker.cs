using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;

internal interface IConnectionTracker
{
    internal Task TrackAsync(HubCallerContext context);
    internal Task UntrackAsync(HubCallerContext context);
}

internal interface IConnectionTracker<THub> : IConnectionTracker where THub : AbstractManagedHub {}
