using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Core;


/// <summary>
/// Proxy class that wraps <see cref="IHubCallerClients"/> available within a <see cref="ManagedHub"/>
/// </summary>
public class HubClientsProxy 
{
    private readonly IHubClients<IManagedHubClient> _hubClients;
    private readonly Type _hubType;

    public HubClientsProxy(IHubClients<IManagedHubClient> hubClients, Type hubType)
    {
        _hubClients = hubClients;
        _hubType = hubType;
    }

    public ManagedHubClientProxy All => new(_hubClients.All, _hubType);
    public ManagedHubClientProxy Client(string connectionId) => new(_hubClients.Client(connectionId), _hubType);
    public ManagedHubClientProxy Clients(IReadOnlyList<string> connectionIds) => new(_hubClients.Clients(connectionIds), _hubType);
    public ManagedHubClientProxy Group(string groupName) => new(_hubClients.Group(groupName), _hubType);
    public ManagedHubClientProxy Groups(IReadOnlyList<string> groupNames) => new(_hubClients.Groups(groupNames), _hubType);
    public ManagedHubClientProxy User(string userId) => new(_hubClients.User(userId), _hubType);
    public ManagedHubClientProxy Users(IReadOnlyList<string> userIds) => new(_hubClients.Users(userIds), _hubType);
}
