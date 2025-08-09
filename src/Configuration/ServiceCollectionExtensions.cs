using Microsoft.Extensions.DependencyInjection;
using ManagedLib.ManagedSignalR.Abstractions;
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
        //configure the hub configuration
        var configuration = new ManagedSignalRConfiguration(services);
        configurer.Invoke(configuration);

        // The SINGLETON configuration is used to retrieve the hub mappings and other settings
        services.AddSingleton(configuration);



        // register the IHubCommandDispatcher
        services.AddScoped<IHubCommandDispatcher, HubCommandDispatcher>();

        // Register the IConnectionManager as an open generic singleton set for each hub
        services.AddSingleton(typeof(IConnectionManager<>), typeof(ConnectionManager<>));





        // Register the managed hub helper based on the deployment mode
        if (configuration.DeploymentMode is null)
        {
            throw new MisconfiguredException(message:
                                                "Deployment mode is not set." +
                                                "Please configure the system by calling 'AsSingleInstance()' or 'AsDistributed()' within the provided configurer."
            );
        }
        else if (configuration.DeploymentMode is DeploymentMode.SingleInstance)
        {
            // register the single-instance managed hub helper 
            services.AddScoped(typeof(IManagedHubHelper<>), typeof(SingleInstanceManagedHubHelper<>));
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