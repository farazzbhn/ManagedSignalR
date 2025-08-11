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
        Action<FrameworkOptions> configurer
    )
    {
        // Configure the framework options 
        FrameworkOptions frameworkOptions = new FrameworkOptions(services);
        configurer.Invoke(frameworkOptions);

        // Invoke the finalize method to seal and rid the object of unnecessary references
        frameworkOptions.Seal();
        FrameworkOptions.Instance = frameworkOptions;

        services.AddScoped<IHubCommandDispatcher, HubCommandDispatcher>();


        services.AddSignalR(hubOptions =>
        {
            if (frameworkOptions.EnableDetailedErrors.HasValue)
            {
                hubOptions.EnableDetailedErrors = frameworkOptions.EnableDetailedErrors.Value;
            }

            if (frameworkOptions.KeepAliveInterval.HasValue)
            {
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(frameworkOptions.KeepAliveInterval.Value);
            }

            if (frameworkOptions.SupportedProtocols is not null && frameworkOptions.SupportedProtocols.Count > 0)
            {
                frameworkOptions.SupportedProtocols = hubOptions.SupportedProtocols;
            }
        });



        return services;
    }
}