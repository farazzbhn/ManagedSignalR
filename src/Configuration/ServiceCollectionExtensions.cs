using Microsoft.Extensions.DependencyInjection;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Implementations;

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

        /// 
        // Create and configure the hub configuration
        var configuration = new ManagedSignalRConfiguration(services);
        configurer.Invoke(configuration);


        // Register core services
        services.AddSingleton(configuration);

        services.AddSingleton<HubCommandDispatcher>();

        // Register the default cache provider
        services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();
        services.AddScoped<IDistributedLockProvider, DistributedLockProvider>();

        // Configure SignalR
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });


        // Register the managed hub helper
        //services.AddScoped<Manage;

        // local cache provider is used to persist instance-specific data regarding the connection
        services.AddScoped<LocalCacheProvider<CacheEntry>>();

        services.AddHostedService<CacheEntryBackgroundService>();

        return services;
    }
}