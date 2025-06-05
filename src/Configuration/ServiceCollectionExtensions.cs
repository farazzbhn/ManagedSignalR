using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureManagedHubs
    (
        this IServiceCollection services,
        Action<ManagedHubConfiguration> configurer
    )
    {
        ManagedHubConfiguration configuration = new();
        configurer.Invoke(configuration);

        // register the configuration as singleton
        services.AddSingleton(configuration);

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        // open generic
        services.AddScoped(typeof(ManagedHubHelper<>));

        return services;
    }
}
