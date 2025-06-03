using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR
{

    /// <summary>
    /// Defines the contract for SignalR hubs that require strong typing. <br />
    /// <b>strong-typed hubs should extend the <see cref="ManagedHub{T}"/> where <see cref="{T}"/> is the name of the strongly typed hub.
    /// For example,  <c>ChatHub : ManagedHub&lt;ChatHub&gt;</c><br /></b> <br/>
    /// NOTE: Method <see cref="Whisper"/> is invoked on/from the client side 
    /// </summary>
    public interface IClient
    {
        Task Whisper(string topic, string body) { return Task.CompletedTask; }
        Task Process(string topic, string body);
    }
}
