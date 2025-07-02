using System.ComponentModel;
using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Text.Json;
using ManagedLib.ManagedSignalR.Exceptions;

namespace ManagedLib.ManagedSignalR.Configuration;


public sealed class HubEndpointConfiguration
{
    private readonly IServiceCollection _services;

    public HubEndpointConfiguration
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


    private Dictionary<string, InvokeServerMapping> inbound { get ; set; } = new();


    private Dictionary<Type, InvokeClientMapping> Outbound { get; set; } = new();


    /// <summary>
    /// Configures how messages are sent to clients
    /// </summary>
    /// <typeparam name="T">Message type to send</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public HubEndpointConfiguration ConfigureInvokeClient<T>(Action<InvokeClientMapping<T>> configurer)
    {
        
        var configuration = new InvokeClientMapping<T>();

        configurer.Invoke(configuration);


        configuration.ThrowIfInvalid();


        // a C# type is bound to a topic/serializer
        Outbound[typeof(T)] = configuration;

        return this;
    }

    /// <summary>
    /// Configures how messages are received from clients
    /// </summary>
    /// <typeparam name="TModel">Message type to receive</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public HubEndpointConfiguration ConfigureInvokeServer<TModel>(Action<InvokeServerMapping<TModel>> configurer)
    {

        var configuration = new InvokeServerMapping<TModel>();

        configurer.Invoke(configuration);
        

        configuration.ThrowIfInvalid();

        // a string (topic) is bound to a  C# type/deserializer
        inbound[configuration.Topic] = configuration;

        // And register the scoped handler within the service provider
        _services.AddScoped(configuration.HandlerType);

        return this;
    }

    internal (string Topic, string Payload) Serialize<TModel>(TModel message)
    {
        if (!Outbound.TryGetValue(typeof(TModel), out var mapping))
            throw new MissingConfigurationException($"No configuration found for message type {typeof(TModel)}. Please ensure it is registered with ConfigureInvokeClient<TModel>() method.");
        return (mapping.Topic, mapping.Serialize(message));
    }

    internal dynamic Deserialize(string topic, string payload)
    {
        if (!inbound.TryGetValue(topic, out var mapping))
            throw new MissingConfigurationException($"No configuration found for topic {topic}. Please ensure it is registered with ConfigureInvokeClient<TModel>() method.");

        return mapping.Deserialize(payload);
    }

}






