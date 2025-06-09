using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;


/// <summary>
/// A provider abstraction for configuring the "User ID" for a connection. <br />
/// Invoked on SignalR connection events by <see cref="ManagedHubHelper{T}"/>
/// </summary>
/// <remarks><see cref="IIdentityProvider"/> is used by <see cref="ManagedHubHelper{T}"/> to associate a connection id with a user.</remarks>
public interface IIdentityProvider
{
    /// <summary>
    /// Gets the user ID for the specified connection.
    /// </summary>
    /// <param name="context">The connection to get the user ID for.</param>
    /// <returns>The user ID for the specified connection.</returns>
    public string GetUserId(HubCallerContext context);

}