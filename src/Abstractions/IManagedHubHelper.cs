using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ManagedLib.ManagedSignalR.Abstractions;

public interface IManagedHubHelper<THub> where THub : AbstractManagedHub
{

    /// <summary>
    /// Sends a message to a specific user identified by user ID.
    /// </summary>
    /// <param name="userIdentifier">The target user ID.</param>
    /// <param name="message">The message to send.</param>
    public Task SendToUser(string userIdentifier, object message);

    /// <summary>
    /// Sends a message to a specific connection identified by connection ID.
    /// </summary>
    /// <param name="connectionId">The target connection ID.</param>
    /// <param name="message">The message to send.</param>
    public Task SendToConnection(string connectionId, object message);

}
