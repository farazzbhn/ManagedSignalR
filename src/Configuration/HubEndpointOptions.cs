using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using ManagedLib.ManagedSignalR.Types.Exceptions;

namespace ManagedLib.ManagedSignalR.Configuration;



/// <summary>
/// Holds mappings and other configuration options for a specific SignalR hub endpoint.
/// </summary>
public sealed class HubEndpointOptions
{
    private readonly IServiceCollection _services;
    public readonly ManagedSignalRConfiguration Parent;

    public HubEndpointOptions
    (
        Type hubType,
        ManagedSignalRConfiguration parent,
        IServiceCollection services
    )
    {
        HubType = hubType;
        Parent = parent;
        _services = services;
        _inbound = new();
        _outbound = new();
    }


    /// <summary>
    /// Hub type being configured
    /// </summary>
    internal Type HubType { get; set; }

    private Dictionary<string, InvokeServerMapping> _inbound { get ; set; } 

    private Dictionary<Type, InvokeClientMapping> _outbound { get; set; } 




    /// <summary>
    /// Configures how messages are sent to clients
    /// </summary>
    /// <typeparam name="TOutboundMessage">Message type to send</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public HubEndpointOptions ConfigureInvokeClient<TOutboundMessage>(Action<InvokeClientMapping<TOutboundMessage>> configurer)
    {
        
        var configuration = new InvokeClientMapping<TOutboundMessage>();

        configurer.Invoke(configuration);


        configuration.EnsureIsValid();


        // a C# type is bound to a topic/serializer
        _outbound[typeof(TOutboundMessage)] = configuration;

        return this;
    }

    /// <summary>
    /// Configures how messages are received from clients
    /// </summary>
    /// <typeparam name="TInboundMessage">Message type to receive</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public HubEndpointOptions ConfigureInvokeServer<TInboundMessage>(Action<InvokeServerMapping<TInboundMessage>> configurer)
    {

        var configuration = new InvokeServerMapping<TInboundMessage>();

        configurer.Invoke(configuration);
        

        configuration.EnsureIsValid();

        // a string (topic) is bound to a  C# type/deserializer
        _inbound[configuration.Topic] = configuration;

        // And register the scoped handler within the service provider
        _services.AddScoped(configuration.HandlerType);

        return this;
    }


    /// <summary>
    /// Serializes the provided message into a payload string using the pre-specified serializer and determines the corresponding topic 
    /// based on the <c>type → topic</c> mappings configured on startup.
    /// </summary>
    /// <param name="message">The object to be serialized.</param>
    /// <returns>A tuple containing the topic and the serialized payload.</returns>
    /// <exception cref="MissingConfigurationException">
    /// Thrown if no mapping was found for the message type. 
    /// Ensure the type is registered using <c>ConfigureInvokeClient&lt;TModel&gt;()</c> during startup.
    /// </exception>
    internal (string Topic, string Payload) Serialize<TModel>(TModel message)
    {
        if (!_outbound.TryGetValue(typeof(TModel), out var mapping))
            throw new MissingConfigurationException($"No configuration found for message type {typeof(TModel)}. Please ensure it is registered with ConfigureInvokeClient<TModel>() method.");
        return (mapping.Topic, mapping.Serialize(message));
    }


    /// <summary>
    /// Deserializes the provided payload into the appropriate C# type based on the <c>topic → type </c>mappings configured on startup.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="MissingConfigurationException"></exception>
    internal dynamic Deserialize(string topic, string payload)
    {
        if (!_inbound.TryGetValue(topic, out var mapping))
            throw new MissingConfigurationException($"No configuration found for topic {topic}. Please ensure it is registered with ConfigureInvokeServer<TModel>() method.");

        return mapping.Deserialize(payload);
    }


    internal Type GetHandlerType(string topic)
    {
        if (!_inbound.TryGetValue(topic, out var mapping))
            throw new MissingConfigurationException($"No configuration found for topic {topic}. Please ensure it is registered with ConfigureInvokeServer<TModel>() method.");

        var type = mapping.HandlerType ?? throw new MisconfiguredException(
            $"Handler type not specified for topic {topic}. Please call UseHandler() to register the respective handler");

        return type;
    }

    /// <summary>
    /// Checks if this hub configuration has a specific topic
    /// </summary>
    /// <param name="topic">The topic to check</param>
    /// <returns>True if the topic is configured for this hub</returns>
    internal bool HasTopic(string topic)
    {
        return _inbound.ContainsKey(topic);
    }

    /// <summary>
    /// Returns to the parent <see cref="ManagedSignalRConfiguration"/> builder
    /// to allow configuring additional hubs or global settings.
    /// </summary>
    /// <returns>The parent <see cref="ManagedSignalRConfiguration"/> instance for fluent chaining.</returns>
    public ManagedSignalRConfiguration And() => Parent;

}






