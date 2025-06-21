namespace ManagedLib.ManagedSignalR.Abstractions;
public interface IHubMediator
{
    public Task<bool> SendToConnectionId<THub>(object message, string connectionId) where THub : ManagedHub;
}
