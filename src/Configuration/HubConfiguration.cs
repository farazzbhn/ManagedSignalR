using System.ComponentModel;
using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ManagedLib.ManagedSignalR.Configuration;


public sealed class HubConfiguration
{
    private readonly IServiceCollection _services;

    public HubConfiguration
    (
        Type hubType,
        IServiceCollection services
    )
    {
        _services = services;
        HubType = hubType;
    }


    /// <summary>
    /// Hub type being configured
    /// </summary>
    internal Type HubType { get; set; }


    internal Dictionary<string, InvokeServerConfiguration> Inbound { get ; set; } = new();


    internal Dictionary<Type, InvokeClientConfiguration> Outbound { get; set; } = new();


    /// <summary>
    /// Configures how messages are sent to clients
    /// </summary>
    /// <typeparam name="T">Message type to send</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public HubConfiguration OnInvokeClient<T>(Action<InvokeClientConfiguration<T>> configurer)
    {
        
        var configuration = new InvokeClientConfiguration<T>();

        configurer.Invoke(configuration);


        configuration.ThrowIfInvalid();

        Outbound[typeof(T)] = configuration;

        return this;
    }

    /// <summary>
    /// Configures how messages are received from clients
    /// </summary>
    /// <typeparam name="TModel">Message type to receive</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public HubConfiguration OnInvokeServer<TModel>(Action<InvokeServerConfiguration<TModel>> configurer)
    {

        var configuration = new InvokeServerConfiguration<TModel>();

        configurer.Invoke(configuration);
        

        configuration.ThrowIfInvalid();


        Inbound[configuration.Topic] = configuration;

        // And register the scoped handler within the service provider
        _services.AddScoped(configuration.HandlerType);

        return this;
    }


    public InvokeClientConfiguration FindConfiguration(Type msgType) => Outbound[msgType];
    public InvokeServerConfiguration FindConfiguration(string topic) => Inbound[topic];

}






