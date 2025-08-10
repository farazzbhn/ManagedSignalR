using Microsoft.AspNetCore.SignalR;
using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Core;

// Works only if THub : ManagedHub<IManagedHubClient>
public class ManagedHubContext<THub> : IHubContext<THub, IManagedHubClient> where THub : ManagedHub
{
    private readonly IHubContext<THub, IManagedHubClient> _inner;

    public ManagedHubContext(IHubContext<THub, IManagedHubClient> inner)
    {
        _inner = inner;
    }

    public IHubClients<IManagedHubClient> Clients
        => new HubClientsDecorator(typeof(THub), _inner.Clients);

    public IGroupManager Groups => _inner.Groups;
}

internal class HubClientsDecorator : IHubClients<IManagedHubClient>
{
    private readonly Type _hubType;
    private readonly IHubClients<IManagedHubClient> _inner;

    public HubClientsDecorator(Type hubType, IHubClients<IManagedHubClient> inner)
    {
        _hubType = hubType;
        _inner = inner;
    }


    public IManagedHubClient AllExcept(IReadOnlyList<string> excludedConnectionIds)
    {
        throw new NotImplementedException();
    }

    public IManagedHubClient Client(string connectionId)
    {
        throw new NotImplementedException();
    }

    public IManagedHubClient Clients(IReadOnlyList<string> connectionIds)
    {
        throw new NotImplementedException();
    }

    public IManagedHubClient Group(string groupName)
    {
        throw new NotImplementedException();
    }

    public IManagedHubClient Groups(IReadOnlyList<string> groupNames)
    {
        throw new NotImplementedException();
    }

    public IManagedHubClient GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds)
    {
        throw new NotImplementedException();
    }

    public IManagedHubClient User(string userId)
    {
        throw new NotImplementedException();
    }

    public IManagedHubClient Users(IReadOnlyList<string> userIds)
    {
        throw new NotImplementedException();
    }

    public IManagedHubClient All { get; }
}

public class ClientProxyDecorator : IClientProxy
{
    internal Type HubType;
    private readonly IClientProxy _inner;

    public ClientProxyDecorator(Type hubType, IClientProxy inner)
    {
        HubType = hubType;
        _inner = inner;
    }

    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    {
        // Pre-send hook
        Console.WriteLine($"[{HubType.Name}] Sending {method} with {args.Length} args");
        return _inner.SendCoreAsync(method, args, cancellationToken);
    }

    public Task InvokeClient(string topic, string payload)
    {
        // Optional: intercept InvokeClient
        return _inner.SendCoreAsync(topic, new object?[] { payload }, default);
    }

    public Task InvokeClientAsync<THub>(dynamic message) where THub : ManagedHub
    {
        Console.WriteLine("------------------------------Custom proxy---------------");

        // Optional: intercept typed calls
        return Task.CompletedTask;
    }
}
