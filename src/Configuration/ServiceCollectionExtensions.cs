using Microsoft.Extensions.DependencyInjection;
using ManagedLib.ManagedSignalR.Abstractions;
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
    /// <param name="builder">Configuration builder</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddManagedSignalR
    (
        this IServiceCollection services,
        Action<ManagedSignalRConfiguration> builder
    )
    {
        // Create and configure the hub configuration
        var configuration = new ManagedSignalRConfiguration(services);

        builder.Invoke(configuration);

        // Register core services
        services.AddSingleton(configuration);
        services.AddSingleton<ManagedHubHandlerBus>();

        // Register the default cache provider
        services.AddSingleton<IDistributedCacheProvider, InMemoryCacheProvider>();
        services.AddScoped<IDistributedLockProvider, DistributedLockProvider>();

        // Configure SignalR
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        // Register the managed hub helper
        services.AddScoped<ManagedHubHelper>();

        return services;
    }
}