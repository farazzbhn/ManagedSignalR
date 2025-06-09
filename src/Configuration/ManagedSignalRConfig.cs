using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;

/// <summary>
/// Central configuration for SignalR hubs and their message mappings
/// </summary>
public class ManagedSignalRConfig
{
    internal readonly List<ManagedHubConfig> Configs;

    private readonly IServiceCollection _services;

    public ManagedSignalRConfig
    (
        IServiceCollection services
    )
    {
        Configs = new List<ManagedHubConfig>();
        _services = services;
    }


    /// <summary>
    /// Adds a hub configuration with its message mappings
    /// </summary>
    /// <typeparam name="THub">Hub type to configure</typeparam>
    /// <returns>Configuration builder for the hub</returns>
    public ManagedHubConfig AddHub<THub>() where THub : ManagedHub<THub>
    {
        // Find or create mapping for the hub
        var config = Configs.FirstOrDefault(m => m.HubType == typeof(THub));

        if (config == null)
        {
            config = new ManagedHubConfig(typeof(THub), _services);
            Configs.Add(config);
        }
        return config;
    }

    /// <summary>
    /// Gets configuration for a specific hub type
    /// </summary>
    /// <param name="hubType">Type of hub to get config for</param>
    /// <returns>Hub configuration or null if not found</returns>
    internal ManagedHubConfig? FindManagedHubConfig(Type hubType)
    {
        var config = Configs.SingleOrDefault(x => x.HubType == hubType);
        return config;
    }
}
