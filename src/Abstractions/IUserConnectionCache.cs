using ManagedLib.ManagedSignalR.Core;
namespace ManagedLib.ManagedSignalR.Abstractions;

internal interface IUserConnectionCache
{
    public void AddConnection(Type hubType, string userIdentifier, string connectionId, string instanceId);

    public void RemoveConnection(Type hubType, string userIdentifier, string connectionId);

    public UserConnection[] GetUserConnections(Type hubType, string? userIdentifier);
}
