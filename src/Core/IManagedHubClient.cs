using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Transactions;

namespace ManagedLib.ManagedSignalR.Core;


/// <summary>
/// Base interface for SignalR client communication. <br />
/// Example usage:<br />
/// <c>public class ChatHub : ManagedHub&lt;ChatHub&gt; { }</c>
/// </summary>
public interface IManagedHubClient
{

    /// <summary>
    /// <b>🚫 INTERNAL USE ONLY. 🚫</b> <br/>
    /// <b>Do NOT call this method directly.</b> <br/> 
    /// Instead, use <see cref="ManagedHubClientProxyExtensions.TryInvokeClientAsync"/>.
    /// </summary>
    /// <param name="topic">Message routing topic.</param>
    /// <param name="payload">Serialized message data.</param>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public Task InvokeClient(string topic, string payload) => Task.CompletedTask;
    
}


