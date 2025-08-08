using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;

/// <summary>
/// Central configuration for SignalR hubs and their message mappings
/// </summary>
public class ManagedSignalRConfiguration
{
    private List<EndpointConfiguration> EndpointConfigurations { get; }

    public string CachePrefix { get; set; } = "msr:";

    public bool? EnableDetailedErrors { get; set; } = null;
    public int? KeepAliveInterval { get; set; } = null;
    public IList<string>? SupportedProtocols { get; set; } = null;
    public DeploymentMode? DeploymentMode { get; private set; } = null;


    private readonly IServiceCollection _services;

    public ManagedSignalRConfiguration
    (
        IServiceCollection services
    )
    {
        EndpointConfigurations = new List<EndpointConfiguration>();
        _services = services;
    }


    /// <summary>
    /// Enables detailed errors in SignalR.
    /// </summary>
    public ManagedSignalRConfiguration WithEnabledDetailedErrors()
    {
        EnableDetailedErrors = true;
        return this;
    }

    /// <summary>
    /// Disables detailed errors in SignalR.
    /// </summary>
    public ManagedSignalRConfiguration WithDisabledDetailedErrors()
    {
        EnableDetailedErrors = false;
        return this;
    }


    /// <summary>
    /// Sets the SignalR keep-alive interval in seconds.
    /// </summary>
    /// <param name="interval">The interval, in seconds, at which keep-alive packets are sent to clients.</param>
    /// <returns>The current <see cref="ManagedSignalRConfiguration"/> instance for fluent chaining.</returns>
    public ManagedSignalRConfiguration WithKeepAliveInterval(int interval)
    {
        KeepAliveInterval = interval;
        return this;
    }


    /// <summary>
    /// Sets the supported SignalR protocols.
    /// </summary>
    /// <param name="protocols">List of supported protocol names (e.g., "json", "messagepack").</param>
    /// <returns>The current configuration instance for fluent chaining.</returns>
    public ManagedSignalRConfiguration WithSupportedProtocols(params string[] protocols)
    {
        SupportedProtocols = protocols.ToList();
        return this;
    }







    // DEPLOYMENT MODE CONFIGURATION
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
    /// Adds a managed hub 
    /// </summary>
    /// <typeparam name="THub">Hub type to configure</typeparam>
    /// <returns>Configuration builder for the hub</returns>
    public EndpointConfiguration AddHub<THub>() where THub : AbstractManagedHub
    {
        // Find or create mapping for the hub
        var config = EndpointConfigurations.FirstOrDefault(m => m.HubType == typeof(THub));

        if (config == null)
        {
            config = new EndpointConfiguration(typeof(THub),this, _services);
            EndpointConfigurations.Add(config);
        }
        return config;
    }




    /// <summary>
    /// Finds the <see cref="EndpointConfiguration"/> associated with the SignalR hub of provided type
    /// </summary>
    /// <returns>Hub configuration</returns>
    /// <exception cref="MissingConfigurationException">configuration not found</exception>
    /// <exception cref="InvalidOperationException">invalid input type</exception>
    internal EndpointConfiguration FetchEndpointConfiguration(Type type)
    {
        if (!typeof(AbstractManagedHub).IsAssignableFrom(type))
            throw new InvalidOperationException($"Type {type.FullName} is not a valid ManagedHub type.");

        EndpointConfiguration? config = EndpointConfigurations.SingleOrDefault(x => x.HubType == type);

        if (config is null)
            throw new MissingConfigurationException($"No configuration found for hub type {type.FullName}. Please ensure it is registered with AddHub<THub>() method.");

        return config;
    }

}

public enum DeploymentMode
{
    SingleInstance,
    Distributed
}
