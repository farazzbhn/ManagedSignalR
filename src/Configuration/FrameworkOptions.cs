using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace ManagedLib.ManagedSignalR.Configuration;

/// <summary>
/// Central configuration for managed SignalR hubs 
/// </summary>
public class FrameworkOptions
{

    internal static FrameworkOptions Instance { get;  set; }

    private IServiceCollection? Services { get; set; }
    private List<EndpointOptions> Endpoints { get; set; }


    /// <summary>
    /// Finalizes the instance and rids the object of unnecessary references
    /// </summary>
    internal void Seal()
    {
        Services = null;
        Endpoints!.ForEach(e => e.Seal());
    }


    public FrameworkOptions
    (
        IServiceCollection services
    )
    {
        Endpoints = new List<EndpointOptions>();
        Services = services;
    }


    #region SignalR

    public bool? EnableDetailedErrors { get; set; } = null;
    public int? KeepAliveInterval { get; set; } = null;
    public IList<string>? SupportedProtocols { get; set; } = null;

    /// <summary>
    /// Enables detailed errors in SignalR.
    /// </summary>
    public FrameworkOptions WithEnabledDetailedErrors()
    {
        EnableDetailedErrors = true;
        return this;
    }

    /// <summary>
    /// Disables detailed errors in SignalR.
    /// </summary>
    public FrameworkOptions WithDisabledDetailedErrors()
    {
        EnableDetailedErrors = false;
        return this;
    }


    /// <summary>
    /// Sets the SignalR keep-alive interval in seconds.
    /// </summary>
    /// <param name="interval">The interval, in seconds, at which keep-alive packets are sent to clients.</param>
    /// <returns>The current <see cref="FrameworkOptions"/> instance for fluent chaining.</returns>
    public FrameworkOptions WithKeepAliveInterval(int interval)
    {
        KeepAliveInterval = interval;
        return this;
    }


    /// <summary>
    /// Sets the supported SignalR protocols.
    /// </summary>
    /// <param name="protocols">List of supported protocol names (e.g., "json", "messagepack").</param>
    /// <returns>The current configuration instance for fluent chaining.</returns>
    public FrameworkOptions WithSupportedProtocols(params string[] protocols)
    {
        SupportedProtocols = protocols.ToList();
        return this;
    }


    #endregion






    /// <summary>
    /// Adds a managed hub 
    /// </summary>
    /// <typeparam name="THub">Hub type to configure</typeparam>
    /// <returns>Configuration builder for the hub</returns>
    public EndpointOptions AddManagedHub<THub>() where THub : ManagedHub
    {
        var config = Endpoints.FirstOrDefault(m => m.HubType == typeof(THub));
        if (config == null)
        {
            config = new EndpointOptions(typeof(THub), this, Services);
            Endpoints.Add(config);

            // Register THub with custom factory
            Services.AddTransient<THub>(sp =>
            {
                // instantiate using the param-less constructor
                THub hub = ActivatorUtilities.CreateInstance<THub>(sp);

                // proceed to resolve & set internal dependencies on the hub instance
                IHubCommandDispatcher dispatcher = sp.GetRequiredService<IHubCommandDispatcher>();

                hub.Dispatcher = dispatcher;

                return hub;
            });
        }
        return config;
    }


    /// <summary>
    /// Finds &amp; returns the <see cref="Endpoints"/> associated with the <see cref="ManagedHub"/>> hub of provided concrete type
    /// </summary>
    /// <returns>The matching <see cref="Endpoints"/> as configured at startup.</returns>
    /// <exception cref="MissingConfigurationException">configuration not found</exception>
    /// <exception cref="InvalidOperationException">invalid input type</exception>
    internal EndpointOptions GetEndpointOptions(Type type)
    {
        if (!typeof(ManagedHub).IsAssignableFrom(type))
            throw new InvalidOperationException($"Type {type.FullName} is not a valid ManagedHub type.");

        EndpointOptions? config = Endpoints.SingleOrDefault(x => x.HubType == type);

        if (config is null)
            throw new MissingConfigurationException($"No configuration found for hub type {type.FullName}. Please ensure it is registered with AddManagedHub<THub>() method.");

        return config;
    }

}
