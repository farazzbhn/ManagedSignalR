using System.Runtime.CompilerServices;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Core;


public static class ManagedHubClientProxyExtensions
{
    public static async Task InvokeClientAsync(this IManagedHubClientProxy clientProxy, dynamic message)
    {

        Type hubType = clientProxy.HubType;


        EndpointOptions endpoint = FrameworkOptions.Instance.GetEndpointOptions(hubType);

        if (!endpoint.InvokeClientConfigurations.TryGetValue((Type)message.GetType(), out var route))
            throw new MissingConfigurationException($"No configuration found for message type {message.GetType()}.");

        string topic = route.Topic!;
        string payload = route.Serialize(message);

        await clientProxy.InvokeClient(topic, payload);
    }
}
