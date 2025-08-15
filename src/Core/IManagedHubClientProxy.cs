using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Core;
public interface IManagedHubClientProxy : IManagedHubClient
{
    Type HubType { get; }
}

public class ManagedHubClientProxy : IManagedHubClientProxy
{
    private readonly IManagedHubClient _client;
    public Type HubType { get; }

    public ManagedHubClientProxy(IManagedHubClient client, Type hubType)
    {
        _client = client;
        HubType = hubType;
    }

    // Forwards the call to the underlying SignalR proxy
    public Task InvokeClient(string topic, string payload)
    {
        return _client.InvokeClient(topic, payload);
    }

}
