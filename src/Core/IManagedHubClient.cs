using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.Extensions.Configuration;
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
    /// sends a notification with a topic and serialized payload
    /// </summary>
    /// <param name="topic">Message routing topic</param>
    /// <param name="payload">Serialized` message data</param>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public Task InvokeClient(string topic, string payload) => Task.CompletedTask;


    public async Task<bool> TryInvokeClient<THub>(dynamic message) where THub : ManagedHub
    {
        EndpointOptions endpoint = FrameworkOptions.Instance.GetEndpointOptions(typeof(THub));

        if (!endpoint.InvokeClientConfigurations.TryGetValue((Type)message.GetType(), out InvokeClientConfiguration? route))
            throw new MissingConfigurationException($"No configuration found for message type {typeof(MessageProcessingHandler)}. Please ensure it is registered with ConfigureInvokeClient<TModel>() method.");


        string topic = route.Topic!;
        string payload = route.Serialize(message);

        try
        {
            await InvokeClient(topic, payload);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
