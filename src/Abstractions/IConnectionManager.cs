using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;

public interface IConnectionManager
{
    /// <summary>
    /// <b>Internally used ~ not exposed </b> <br /><br />
    /// Tracks a new connection associated with a user within the context of a specific SignalR hub. <br />
    /// Adds the connection ID to the user's connection set for that hub, creating a new set if needed.
    /// </summary>
    /// <param name="context">The <see cref="HubCallerContext"/> representing the connection to track.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal Task TrackAsync(HubCallerContext context);

    /// <summary>
    /// <b>Internally used ~ not exposed </b><br /><br />
    /// Removes a tracked connection associated with a user within the context of a specific SignalR hub. <br />
    /// Removes the connection ID from the user's connection set for that hub and cleans up if no connections remain.
    /// </summary>
    /// <param name="context">The <see cref="HubCallerContext"/> representing the connection to untrack.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal Task UntrackAsync(HubCallerContext context);

    /// <summary>
    /// Retrieves all active connection IDs associated with the specified user within the context of a specific SignalR hub.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>a task representing an array of active connection IDs linked to the user within the hub.</returns>
    /// <remarks>
    /// If <paramref name="userIdentifier"/> is null, returns connection IDs
    /// that are anonymous or not associated with any user within the hub context.
    /// </remarks>
    public string[] UserConnections(string? userIdentifier);

    //public Task AddToRoomAsync(string? connectionId, string roomId);

    //public Task RemoveFromRoomAsync(string? connectionId, string roomId);

}

internal interface IConnectionManager<THub> : IConnectionManager where THub : ManagedHub { }