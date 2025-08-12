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
    /// Adds and configures Managed SignalR hubs to the service collection.
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
        // register the options as a static singleton so that extension methods can access the option without DI
        FrameworkOptions.Instance = frameworkOptions;

        //When a class asks for IManagedHubContext<SomeManagedHub>, provide an instance of ManagedHubContext<SomeManagedHub>
        services.AddSingleton(typeof(IManagedHubContext<>), typeof(ManagedHubContext<>));

        // and the command dispatcher which is set & not injected into managedHubs
        services.AddScoped<IHubCommandDispatcher, HubCommandDispatcher>();
        return services;
    }
}