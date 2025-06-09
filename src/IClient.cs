using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR
{
    /// <summary>
    /// Base interface for SignalR client communication.
    /// Example usage:
    /// public class ChatHub : ManagedHub<ChatHub> { }
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// sends a notification with a topic and serialized payload
        /// </summary>
        /// <param name="topic">Message routing topic</param>
        /// <param name="payload">Serialized message data</param>
        Task FireClient(string topic, string payload) => Task.CompletedTask;
    }
}
