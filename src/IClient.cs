using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR
{
    /// <summary>
    /// Defines the contract for SignalR hubs that require strong typing.<br/>
    /// Strongly typed hubs need implement <see cref="ManagedHub{T}"/> where <c>T</c> is the strongly typed hub. For example : <br />
    /// <c>ChatHub : ManagedHub&lt;ChatHub&gt;</c>
    /// </summary>
    public interface IClient
    {
        Task Push(string topic, string payload) => Task.CompletedTask;

        /// <summary>
        /// Invoked on the client side to submit a message to the server. <br />
        /// Implemented in <see cref="ManagedHub{T}"/>.<br />
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task Process(string topic, string payload);
    }
}
