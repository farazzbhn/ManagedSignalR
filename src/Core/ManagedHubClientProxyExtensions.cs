using System.Runtime.CompilerServices;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Core;


public static class ManagedHubClientProxyExtensions
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientProxy"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    /// <exception cref="MissingConfigurationException"></exception>
    public static async Task<bool> TryInvokeClientAsync(this IManagedHubClientProxy clientProxy, dynamic message)
    {

        Type hubType = clientProxy.HubType;


        EndpointOptions endpoint = ManagedSignalROptions.Instance.GetEndpointOptions(hubType);

        if (!endpoint.InvokeClientConfigurations.TryGetValue((Type)message.GetType(), out var route))
            throw new MissingConfigurationException(
                $"No configuration found for outgoing message type {message.GetType()} within hub {nameof(hubType)}.");

        string topic = route.Topic!;
        string payload = route.Serialize(message);

        try
        {
            await clientProxy.InvokeClient(topic, payload);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}
