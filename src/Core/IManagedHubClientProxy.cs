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
        public ManagedHubClientProxy Caller => new(_hub.Clients.Caller, _hubType);
        public ManagedHubClientProxy Others => new(_hub.Clients.Others, _hubType);
        public ManagedHubClientProxy OthersInGroup(string groupName) => new(_hub.Clients.OthersInGroup(groupName), _hubType);
        public ManagedHubClientProxy Client(string connectionId) => new(_hub.Clients.Client(connectionId), _hubType);
        public ManagedHubClientProxy Clients(IReadOnlyList<string> connectionIds) => new(_hub.Clients.Clients(connectionIds), _hubType);
        public ManagedHubClientProxy Group(string groupName) => new(_hub.Clients.Group(groupName), _hubType);
        public ManagedHubClientProxy Groups(IReadOnlyList<string> groupNames) => new(_hub.Clients.Groups(groupNames), _hubType);
        public ManagedHubClientProxy User(string userId) => new(_hub.Clients.User(userId), _hubType);
        public ManagedHubClientProxy Users(IReadOnlyList<string> userIds) => new(_hub.Clients.Users(userIds), _hubType);
    }
}

}
