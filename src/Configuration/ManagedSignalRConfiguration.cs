using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Implementations;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;

/// <summary>
/// Central configuration for SignalR hubs and their message mappings
/// </summary>
public class ManagedSignalRConfiguration
{
    internal DeploymentMode? DeploymentMode { get; private set; } = null;

    private List<HubEndpointConfiguration> Configurations { get; }


    private readonly IServiceCollection _services;

    public ManagedSignalRConfiguration
    (
        IServiceCollection services
    )
    {
        Configurations = new List<HubEndpointConfiguration>();
        _services = services;
    }


    /// <summary>
    /// Adds a managed hub 
    /// </summary>
    /// <typeparam name="THub">Hub type to configure</typeparam>
    /// <returns>Configuration builder for the hub</returns>
    public HubEndpointConfiguration AddHub<THub>() where THub : ManagedHub
    {
        // Find or create mapping for the hub
        var config = Configurations.FirstOrDefault(m => m.HubType == typeof(THub));

        if (config == null)
        {
            config = new HubEndpointConfiguration(typeof(THub), _services);
            Configurations.Add(config);
        }
        return config;
    }


    public ManagedSignalRConfiguration AsSingleInstance()
    {
        DeploymentMode = Configuration.DeploymentMode.SingleInstance;
        return this;
    }


    public ManagedSignalRConfiguration AsDistributed()
    {
        DeploymentMode = Configuration.DeploymentMode.Distributed;
        return this;
    }



    /// <summary>
    /// Finds the <see cref="HubEndpointConfiguration"/> associated with the SignalR hub of provided type
    /// </summary>
    /// <returns>Hub configuration</returns>
    /// <exception cref="MissingConfigurationException">configuration not found</exception>
    /// <exception cref="InvalidOperationException">invalid input type</exception>
    internal HubEndpointConfiguration GetConfiguration(Type type)
    {
        if (!typeof(ManagedHub).IsAssignableFrom(type))
            throw new InvalidOperationException($"Type {type.FullName} is not a valid ManagedHub type.");

        HubEndpointConfiguration? config = Configurations.SingleOrDefault(x => x.HubType == type);

        if (config is null)
            throw new MissingConfigurationException($"No configuration found for hub type {type.FullName}. Please ensure it is registered with AddHub<THub>() method.");

        return config;
    }

}

internal enum DeploymentMode
{
    SingleInstance,
    Distributed
}
