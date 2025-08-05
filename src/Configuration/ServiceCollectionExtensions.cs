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
            // Register the distributed managed hub helper
            services.AddScoped<ManagedHubHelper, DistributedManagedHubHelper>();

            // In distributed mode, we use a distributed cache provider to store connection data across instances
            services.AddScoped<IDistributedCache, RedisCache>();

            // Register the cache entry background service to periodically re-instate expiring cache entries
            services.AddHostedService<CacheEntryBackgroundService>();
        }
        else // if (configuration.DeploymentMode is DeploymentMode.SingleInstance)
        {

            // register the single-instance managed hub helper whic works without a distributed cache 
            // using the local memory cache
            services.AddScoped<ManagedHubHelper, InMemoryManagedHubHelper>();

            // NO implementation of IDistributedCacheProvider is needed in single instance mode
            // NO cache entry background service is needed either ( cache entries do not expire in single instance mode )
        }

        services.AddSingleton(configuration);

        services.AddMemoryCache();



        // Register the configuration as a singleton
        services.AddSingleton<ManagedHubCommandDispatcher>();

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