using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;

public interface IConnectionTracker
{
    Task TrackAsync(HubCallerContext context);
    Task UntrackAsync(HubCallerContext context);
}

public interface IConnectionTracker<THub> : IConnectionTracker where THub : AbstractManagedHub 
{
}
