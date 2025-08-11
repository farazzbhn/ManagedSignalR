using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    // Forward the call to the underlying SignalR proxy
    public Task InvokeClient(string topic, string payload)
    {
        return _client.InvokeClient(topic, payload);
    }

    // A context class to provide easy access to all client proxies
    public class HubContextProxy
    {
        private readonly Hub<IManagedHubClient> _hub;
        private readonly Type _hubType;

        public HubContextProxy(Hub<IManagedHubClient> hub, Type hubType)
        {
            _hub = hub;
            _hubType = hubType;
        }

        public ManagedHubClientProxy All => new(_hub.Clients.All, _hubType);
        public ManagedHubClientProxy Others => new(_hub.Clients.Others, _hubType);
        public ManagedHubClientProxy Caller => new(_hub.Clients.Caller, _hubType);

        // You can add more as needed, like .Group(), .Client(), etc.
    }

}
