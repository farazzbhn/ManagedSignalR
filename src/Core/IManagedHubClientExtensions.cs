using System.Runtime.CompilerServices;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Types.Exceptions;

namespace ManagedLib.ManagedSignalR.Core;


public static class IManagedHubClientExtensions
{
    public static async Task InvokeClientAsync(this IManagedHubClient client, dynamic message)
    {

        Type hubType = ((dynamic)client).HubType;

        EndpointOptions endpoint = FrameworkOptions.Instance.GetEndpointOptions(hubType);

        if (!endpoint.InvokeClientConfigurations.TryGetValue((Type)message.GetType(), out var route))
            throw new MissingConfigurationException($"No configuration found for message type {message.GetType()}.");

        string topic = route.Topic!;
        string payload = route.Serialize(message);
    }
}
