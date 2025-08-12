using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Core;


/// <summary>
/// Proxy class that wraps <see cref="IHubCallerClients"/> available within a <see cref="ManagedHub"/>
/// </summary>
public class HubCallerClientProxy
{
    private readonly IHubCallerClients<IManagedHubClient> _hubCallerClients;
    private readonly Type _hubType;

    public HubCallerClientProxy(IHubCallerClients<IManagedHubClient> hubCallerClients, Type hubType)
    {
        _hubCallerClients = hubCallerClients;
        _hubType = hubType;
    }

    public ManagedHubClientProxy All => new(_hubCallerClients.All, _hubType);
    public ManagedHubClientProxy Caller => new(_hubCallerClients.Caller, _hubType);
    public ManagedHubClientProxy Others => new(_hubCallerClients.Others, _hubType);
    public ManagedHubClientProxy OthersInGroup(string groupName) => new(_hubCallerClients.OthersInGroup(groupName), _hubType);
    public ManagedHubClientProxy Client(string connectionId) => new(_hubCallerClients.Client(connectionId), _hubType);
    public ManagedHubClientProxy Clients(IReadOnlyList<string> connectionIds) => new(_hubCallerClients.Clients(connectionIds), _hubType);
    public ManagedHubClientProxy Group(string groupName) => new(_hubCallerClients.Group(groupName), _hubType);
    public ManagedHubClientProxy Groups(IReadOnlyList<string> groupNames) => new(_hubCallerClients.Groups(groupNames), _hubType);
    public ManagedHubClientProxy User(string userId) => new(_hubCallerClients.User(userId), _hubType);
    public ManagedHubClientProxy Users(IReadOnlyList<string> userIds) => new(_hubCallerClients.Users(userIds), _hubType);
}
