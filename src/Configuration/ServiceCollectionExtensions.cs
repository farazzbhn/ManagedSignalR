using Microsoft.Extensions.DependencyInjection;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Implementations;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Configuration;

/// <summary>
/// Extension methods for configuring SignalR services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures SignalR services with managed connection handling
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configurer">Configuration builder</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddManagedSignalR
    (
        this IServiceCollection services,
        Action<ManagedSignalRConfiguration> configurer
    )
    {
        // GetTracker and configure the hub configuration
        var configuration = new ManagedSignalRConfiguration(services);
        configurer.Invoke(configuration);

        // Register the configuration as a singleton. The configuration is used to retrieve the hub mappings and other settings
        services.AddSingleton(configuration);

        // Register the connection tracker as an open generic singleton
        services.AddSingleton(typeof(IConnectionTracker<>), typeof(ConnectionTracker<>));

        // Register the hub command dispatcher
        services.AddScoped<IHubCommandDispatcher, HubCommandDispatcher>();

        // Register the managed hub helper based on the deployment mode

        if (configuration.DeploymentMode is null)
        {
            var msg = "Deployment mode is not set. Please configure the system by calling 'AsSingleInstance()' or 'AsDistributed()' within the provided configurer.";

            throw new MisconfiguredException(msg);

        }
        else if (configuration.DeploymentMode is DeploymentMode.SingleInstance)
        {
            // register the single-instance managed hub helper 
            services.AddScoped<IManagedHubHelper<>, ManagedHubHelper<>>();

            // Register the distributed managed hub helper
            //services.AddScoped<ManagedHubHelper, DistributedManagedHubHelper>();
        }
        else // if (configuration.DeploymentMode is DeploymentMode.Distributed)
        {

            // using the local memory cache
        }

        /*********************************
         *     SignalR Configuration     *
         *********************************/
        services.AddSignalR(options =>
        {
            if (configuration.EnableDetailedErrors.HasValue)
            {
                options.EnableDetailedErrors = configuration.EnableDetailedErrors.Value;
            }

            if (configuration.KeepAliveInterval.HasValue)
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(configuration.KeepAliveInterval.Value);
            }

            if (configuration.SupportedProtocols is not null && configuration.SupportedProtocols.Count > 0)
            {
                options.SupportedProtocols = configuration.SupportedProtocols;
            }
        });

        return services;
    }
}