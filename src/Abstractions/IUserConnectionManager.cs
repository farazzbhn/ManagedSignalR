using ManagedLib.ManagedSignalR.Core;
namespace ManagedLib.ManagedSignalR.Abstractions;

public interface IUserConnectionManager
{
    public void AddConnection(Type hubType, string userIdentifier, string connectionId, string instanceId);

    public void RemoveConnection(Type hubType, string userIdentifier, string connectionId);

    public UserConnection[] GetUserConnections(Type hubType, string userIdentifier);
}
