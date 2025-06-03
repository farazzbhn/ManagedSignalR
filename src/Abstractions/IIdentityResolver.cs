using ManagedLib.ManagedSignalR.Helper;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Invoked on SignalR connection events by <see cref="ManagedHubHelper{T}"/>,
/// the class is used to identify the user id associated with the connection/http context. <br />
/// The id can be later passed to <see cref="ManagedHubHelper{T}.TryWhisper"/> to send fan-out messages to the user's active connections.
/// </summary>
public interface IIdentityResolver
{
    /// <summary>
    /// Invoked on SignalR connection events, the method must return a unique identifier for the user associated with the connection. <br />
    /// The id can be later passed to <see cref="ManagedHubHelper{T}.TryWhisper"/> to send fan-out messages to user's active connections.
    /// </summary>
    /// <remarks>
    /// The returned ID must be :<br />
    /// - Consistent for the same user across the system<br />
    /// - Unique per user <br />
    /// </remarks>
    /// <param name="context">The hub connection context containing user authentication and connection details.</param>
    /// <returns>A unique identifier string for the connected user.</returns>
    Task<string> GetUserId(HubCallerContext context);
}