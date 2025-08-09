namespace ManagedLib.ManagedSignalR.Abstractions;

public interface IManagedHubHelper
{
    /// <summary>
    /// Sends a message to a specific user identified by user ID as identified by <see cref="IHubCallerContext"/>.<br />
    /// </summary>
    /// <param name="userIdentifier">The target user ID</param>
    /// <param name="message">The message to send.</param>
    public Task SendToUserAsync(object message, string? userIdentifier, int? maxConcurrency = null);

    /// <summary>
    /// Sends a message to a specific connection identified by connection ID.
    /// </summary>
    /// <param name="connectionId">The target connection ID.</param>
    /// <param name="message">The message to send.</param>
    public Task SendToConnectionAsync(object message, string connectionId);

}

internal interface IManagedHubHelper<THub> : IManagedHubHelper where THub : ManagedHub {}
