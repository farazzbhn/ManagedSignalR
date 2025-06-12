namespace ManagedLib.ManagedSignalR.Abstractions;
public interface IManagedHubHelper
{
    public Task<int> SendToUser<THub>(object message, string userId) where THub : ManagedHub;
    public Task<bool> SendToConnection<THub>(object message, string connectionId) where THub : ManagedHub;
}
