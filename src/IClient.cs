using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR
{
    /// <summary>
    /// Defines the contract for SignalR hubs that support strong typing.<br/>
    /// Implementers of strongly typed hubs should derive from <see cref="ManagedHub{T}"/>, 
    /// where <typeparamref name="T"/> is the concrete hub type.<br/>
    /// For example:<br/>
    /// <code>public class ChatHub : ManagedHub&lt;ChatHub&gt; { }</code>
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Sends a notification message to the client with the specified topic and payload. 
        /// 
        /// </summary>
        /// <param name="topic">The topic or event name used for routing the message.</param>
        /// <param name="payload">The serialized message payload.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task NotifyClient(string topic, string payload) => Task.CompletedTask;
    }
}
