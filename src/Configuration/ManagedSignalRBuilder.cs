using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;

/// <summary>
/// Central configuration for SignalR hubs and their message mappings
/// </summary>
public class ManagedSignalRBuilder
{
    internal List<HubConfiguration> Configurations { get; }

    private readonly IServiceCollection _services;

    public ManagedSignalRBuilder
    (
        IServiceCollection services
    )
    {
        Configurations = new List<HubConfiguration>();
        _services = services;
    }


    /// <summary>
    /// Adds a managed hub 
    /// </summary>
    /// <typeparam name="THub">Hub type to configure</typeparam>
    /// <returns>Configuration builder for the hub</returns>
    public HubConfiguration AddHub<THub>() where THub : ManagedHub
    {
        // Find or create mapping for the hub
        var config = Configurations.FirstOrDefault(m => m.HubType == typeof(THub));

        if (config == null)
        {
            config = new HubConfiguration(typeof(THub), _services);
            Configurations.Add(config);
        }
        return config;
    }

    /// <summary>
    /// Finds the <see cref="HubConfiguration"/> associate with the SignalR hub of provided type
    /// </summary>
    /// <param name="hubType">Type of hub to get config for</param>
    /// <returns>Hub configuration or null if not found</returns>
    internal HubConfiguration? GetInvokeOnServerConfiguration(Type hubType)
    {
        HubConfiguration? config = Configurations.SingleOrDefault(x => x.HubType == hubType).;
        return config;
    }
}
