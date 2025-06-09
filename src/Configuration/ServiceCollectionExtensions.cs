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
    /// <param name="configurer">Configuration builder</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddManagedSignalR
    (
        this IServiceCollection services,
        Action<ManagedSignalRConfig> configurer
    )
    {
        // Create and configure the hub configuration
        var configuration = new ManagedSignalRConfig(services);

        configurer.Invoke(configuration);

        // Register core services
        services.AddSingleton(configuration);
        services.AddSingleton<HandlerBus>();

        // Register the default cache provider
        services.AddSingleton<ICacheProvider, DefaultCacheProvider>();
        services.AddScoped<IIdentityProvider, DefaultIdentityProvider>();

        // Configure SignalR
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        // Register the hub helper as scoped open generic
        services.AddScoped(typeof(ManagedHubHelper<>));

        return services;
    }
}