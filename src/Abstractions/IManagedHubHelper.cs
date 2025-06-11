namespace ManagedLib.ManagedSignalR.Abstractions;
public interface IManagedHubHelper
{
    public Task<int> InvokeClientAsync<THub>(object message, string userId) where THub : ManagedHub;
    public Task<int> InvokeClientAsync<THub>(object message, string[] connectionIds) where THub : ManagedHub;
    public Task<string[]> GetConnectionIds(string userId);

}
