using Microsoft.AspNetCore.SignalR;
using System.Reflection;

namespace ManagedLib.ManagedSignalR.Core;

public class ManagedHubContext<THub> : IHubContext<THub> where THub : Hub
{
    private readonly IHubContext<THub> _wrapped;

    public ManagedHubContext(IHubContext<THub> inner) => _wrapped = inner;

    public IHubClients Clients => new HubClientsDecorator(typeof(THub), _wrapped.Clients);

    public IGroupManager Groups => _wrapped.Groups;
}

internal class HubClientsDecorator : IHubClients
{
    private readonly Type _hubType;
    private readonly IHubClients _wrapped;


    public HubClientsDecorator(Type hubType, IHubClients inner)
    {
        _hubType = hubType;
        _wrapped = inner;
    }

    public IClientProxy All => new ClientProxyDecorator(_hubType, _wrapped.All);

    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => 
        new ClientProxyDecorator(_hubType, _wrapped.AllExcept(excludedConnectionIds));

    public IClientProxy Client(string connectionId) =>
        new ClientProxyDecorator(_hubType, _wrapped.Client(connectionId));

    public IClientProxy Clients(IReadOnlyList<string> connectionIds) =>
        new ClientProxyDecorator(_hubType, _wrapped.Clients(connectionIds));

    public IClientProxy Group(string groupName) =>
        new ClientProxyDecorator(_hubType, _wrapped.Group(groupName));

    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) =>
        new ClientProxyDecorator(_hubType, _wrapped.GroupExcept(groupName, excludedConnectionIds));

    public IClientProxy Groups(IReadOnlyList<string> groupNames) =>
        new ClientProxyDecorator(_hubType, _wrapped.Groups(groupNames));

    public IClientProxy User(string userId) =>
        new ClientProxyDecorator(_hubType, _wrapped.User(userId));

    public IClientProxy Users(IReadOnlyList<string> userIds) =>
        new ClientProxyDecorator(_hubType, _wrapped.Users(userIds));
}

public class ClientProxyDecorator : IClientProxy
{
    internal Type HubType;
    private readonly IClientProxy _wrapped;


    public ClientProxyDecorator(Type hubType, IClientProxy inner)
    {
        HubType = hubType;
        _wrapped = inner;
    }

    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = new CancellationToken())
        => _wrapped.SendCoreAsync(method, args, cancellationToken);
}