namespace ManagedLib.ManagedSignalR.Abstractions;

public interface IManagedHubHelper<THub> where THub : AbstractManagedHub
{

    /// <summary>
    /// Sends a message to a specific user identified by user ID.
    /// </summary>
    /// <param name="userIdentifier">The target user ID.</param>
    /// <param name="message">The message to send.</param>
    public Task SendToUserAsync(object message, string? userIdentifier, int? maxConcurrency = null);


    /// <summary>
    /// Sends a message to a specific connection identified by connection ID.
    /// </summary>
    /// <param name="connectionId">The target connection ID.</param>
    /// <param name="message">The message to send.</param>
    public Task SendToConnectionIdAsync(object message, string connectionId);


    /// <summary>
    /// Retrieves all active connection IDs associated with the specified user within the context of a specific SignalR hub.
    /// </summary>
    /// <param name="userIdentifier">The unique identifier of the user.</param>
    /// <returns>An array of active connection IDs linked to the user within the hub.</returns>
    /// <remarks>
    /// If <paramref name="userIdentifier"/> is null, returns connection IDs
    /// that are anonymous or not associated with any user within the hub context.
    /// </remarks>
    public Task<string[]> ListConnectionIdsAsync(string userIdentifier);

}
