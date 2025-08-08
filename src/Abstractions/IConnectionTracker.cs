using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;

public interface IConnectionTracker
{
    internal Task TrackAsync(HubCallerContext context);
    internal Task UntrackAsync(HubCallerContext context);
}

public interface IConnectionTracker<THub> : IConnectionTracker where THub : AbstractManagedHub 
{
}
