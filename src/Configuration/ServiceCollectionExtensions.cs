using Microsoft.Extensions.DependencyInjection;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Implementations;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

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
        // Create and configure the hub configuration
        var configuration = new ManagedSignalRConfiguration(services);
        configurer.Invoke(configuration);

        if (configuration.DeploymentMode is null)
        {
            throw new MisconfiguredException(
                $"Deployment mode is not set.\n" +
                $"To fix this, configure the system by calling 'AsSingleInstance()' or 'AsDistributed() within the provided configurer'");

        }
        else if (configuration.DeploymentMode is DeploymentMode.Distributed)
        {
            // use the distributed managed hub helper
            services.AddScoped<ManagedHubHelper, DistributedManagedHubHelper>();

            // use redis for multi-instance cache
            services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();

            // local cache provider is used to persist instance-bound data to be used locally
            services.AddScoped<LocalCacheProvider<ManagedHubSessionCacheEntry>>();

            // register the cache entry background service to re-cache instance-bound connection data before they expire
            services.AddHostedService<CacheEntryBackgroundService>();
        }
        else // if (configuration.DeploymentMode is DeploymentMode.SingleInstance)
        {

            // In-memory cache is used to store connection data LOCALLY
            services.AddMemoryCache(); 
            services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();

            // register the single-instance managed hub helper
            services.AddScoped<ManagedHubHelper, SingleInstanceManagedHubHelper>();
        }



        // Register the configuration as a singleton
        services.AddSingleton(configuration);
        services.AddSingleton<HubCommandDispatcher>();

        // Configure SignalR
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