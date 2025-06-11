namespace ManagedLib.ManagedSignalR.Abstractions;
public interface IManagedHubHelper
{

    /// <summary>
    /// Oon each connection 
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="userId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task<int> InvokeClientAsync<THub, TMessage>(string userId, TMessage message) where THub : ManagedHub;
}
