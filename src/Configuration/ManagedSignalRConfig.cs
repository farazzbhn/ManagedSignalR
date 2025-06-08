using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;

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
    /// Configures a hub with its event mappings
    /// </summary>
    /// <typeparam name="THub">The hub type that inherits from ManagedHub</typeparam>
    /// <returns>An EventMapping instance for fluent configuration</returns>
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

    internal ManagedHubConfig? GetConfig(Type hubType)
    {
        var config = Configs.SingleOrDefault(x => x.HubType == hubType);
        return config;
    }
}
