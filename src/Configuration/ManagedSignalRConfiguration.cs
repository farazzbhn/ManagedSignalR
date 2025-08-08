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
    private List<HubEndpointOptions> Options { get; }

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
        Options = new List<HubEndpointOptions>();
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
    public HubEndpointOptions AddHub<THub>() where THub : ManagedHub
    {
        // Find or create mapping for the hub
        var config = Options.FirstOrDefault(m => m.HubType == typeof(THub));

        if (config == null)
        {
            config = new HubEndpointOptions(typeof(THub),this, _services);
            Options.Add(config);
        }
        return config;
    }





    /// <summary>
    /// Finds the <see cref="HubEndpointOptions"/> associated with the SignalR hub of provided type
    /// </summary>
    /// <returns>Hub configuration</returns>
    /// <exception cref="MissingConfigurationException">configuration not found</exception>
    /// <exception cref="InvalidOperationException">invalid input type</exception>
    internal HubEndpointOptions GetHubEndpointOptions(Type type)
    {
        if (!typeof(ManagedHub).IsAssignableFrom(type))
            throw new InvalidOperationException($"Type {type.FullName} is not a valid ManagedHub type.");

        HubEndpointOptions? config = Options.SingleOrDefault(x => x.HubType == type);

        if (config is null)
            throw new MissingConfigurationException($"No configuration found for hub type {type.FullName}. Please ensure it is registered with AddHub<THub>() method.");

        return config;
    }

    /// <summary>
    /// Gets all hub endpoint options for dispatching purposes
    /// </summary>
    /// <returns>All configured hub endpoint options</returns>
    internal IEnumerable<HubEndpointOptions> GetAllHubEndpointOptions()
    {
        return Options.AsReadOnly();
    }
}

public enum DeploymentMode
{
    SingleInstance,
    Distributed
}
