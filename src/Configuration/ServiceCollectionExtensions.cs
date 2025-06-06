using Microsoft.Extensions.DependencyInjection;
using ManagedLib.ManagedSignalR.Implementations;
using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddManagedSignalR
    (
        this IServiceCollection services,
        Action<ManagedHubConfiguration> configurer
    )
    {
        // Create and configure the hub configuration
        var configuration = new ManagedHubConfiguration(services);

        configurer.Invoke(configuration);

        // Register core services
        services.AddSingleton(configuration);
        services.AddSingleton<HandlerBus>();

        //TODO : thing about it
        services.AddSingleton<ICacheProvider, InMemoryCacheProvider>();

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
