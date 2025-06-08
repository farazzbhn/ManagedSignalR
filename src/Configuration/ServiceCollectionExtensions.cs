using Microsoft.Extensions.DependencyInjection;
using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Configuration;

public static class ServiceCollectionExtensions
{
    // Accept a factory delegate that returns ICacheProvider
    public static IServiceCollection AddManagedSignalR
    (
        this IServiceCollection services,
        Action<ManagedSignalRConfig> configurer,
        Func<IServiceProvider, ICacheProvider> cacheProviderFactory
    )
    {
        // Create and configure the hub configuration
        var configuration = new ManagedSignalRConfig(services);

        configurer.Invoke(configuration);

        // Register core services
        services.AddSingleton(configuration);
        services.AddSingleton<HandlerBus>();

        // Register ICacheProvider using the factory delegate
        services.AddSingleton<ICacheProvider>(cacheProviderFactory);

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