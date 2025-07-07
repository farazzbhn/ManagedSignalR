using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR;


/// <summary>
/// Base interface for SignalR client communication. <br />
/// Example usage:<br />
/// <c>public class ChatHub : ManagedHub&lt;ChatHub&gt; { }</c>
/// </summary>
public interface IManagedHubClient
{
    /// <summary>
    /// sends a notification with a topic and serialized payload
    /// </summary>
    /// <param name="topic">Message routing topic</param>
    /// <param name="payload">Serialized message data</param>
    Task InvokeClient(string topic, string payload) => Task.CompletedTask;
}
