using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;

/// <summary>
/// Central configuration for SignalR hubs and their message mappings
/// </summary>
public class GlobalConfiguration
{
    internal List<ManagedHubConfiguration> Configurations { get; }

    private readonly IServiceCollection _services;

    public GlobalConfiguration
    (
        IServiceCollection services
    )
    {
        Configurations = new List<ManagedHubConfiguration>();
        _services = services;
    }


    /// <summary>
    /// Adds a managed hub 
    /// </summary>
    /// <typeparam name="THub">Hub type to configure</typeparam>
    /// <returns>Configuration builder for the hub</returns>
    public ManagedHubConfiguration AddHub<THub>() where THub : ManagedHub
    {
        // Find or create mapping for the hub
        var config = Configurations.FirstOrDefault(m => m.HubType == typeof(THub));

        if (config == null)
        {
            config = new ManagedHubConfiguration(typeof(THub), _services);
            Configurations.Add(config);
        }
        return config;
    }

    /// <summary>
    /// Finds the <see cref="ManagedHubConfiguration"/> associate with the SignalR hub of provided type
    /// </summary>
    /// <param name="hubType">Type of hub to get config for</param>
    /// <returns>Hub configuration or null if not found</returns>
    internal ManagedHubConfiguration? FindConfiguration(Type hubType)
    {
        ManagedHubConfiguration? config = Configurations.SingleOrDefault(x => x.HubType == hubType);
        return config;
    }
}
