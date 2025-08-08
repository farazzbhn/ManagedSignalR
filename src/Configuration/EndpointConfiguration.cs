using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;


/// <summary>
/// Holds mappings and other configuration options for a specific SignalR hub endpoint.
/// </summary>
public sealed class EndpointConfiguration
{
    private readonly IServiceCollection _services;
    public readonly ManagedSignalRConfiguration Parent;

    public EndpointConfiguration
    (
        Type hubType,
        ManagedSignalRConfiguration parent,
        IServiceCollection services
    )
    {
        HubType = hubType;
        Parent = parent;
        _services = services;
        _invokeServerConfigurations = new();
        _invokeClientConfigurations = new();
    }


    /// <summary>
    /// Hub type being configured
    /// </summary>
    internal Type HubType { get; set; }

    private Dictionary<string, InvokeServerConfiguration> _invokeServerConfigurations { get; set; }

    private Dictionary<Type, InvokeClientConfiguration> _invokeClientConfigurations { get; set; }

    public IReadOnlyDictionary<string, InvokeServerConfiguration> InvokeServerConfigurations => _invokeServerConfigurations;
    public IReadOnlyDictionary<Type, InvokeClientConfiguration> InvokeClientConfigurations => _invokeClientConfigurations;


    /// <summary>
    /// Configures how messages are sent to clients
    /// </summary>
    /// <typeparam name="TOutboundMessage">Message type to send</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public EndpointConfiguration ConfigureInvokeClient<TOutboundMessage>(Action<InvokeClientConfiguration<TOutboundMessage>> configurer)
    {

        var configuration = new InvokeClientConfiguration<TOutboundMessage>();

        configurer.Invoke(configuration);


        configuration.EnsureIsValid();


        // a C# type is bound to a topic/serializer
        _invokeClientConfigurations[typeof(TOutboundMessage)] = configuration;

        return this;
    }

    /// <summary>
    /// Configures how messages are received from clients
    /// </summary>
    /// <typeparam name="TInboundMessage">Message type to receive</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public EndpointConfiguration ConfigureInvokeServer<TInboundMessage>(Action<InvokeServerConfiguration<TInboundMessage>> configurer)
    {

        var configuration = new InvokeServerConfiguration<TInboundMessage>();

        configurer.Invoke(configuration);


        configuration.EnsureIsValid();

        // a string (topic) is bound to a  C# type/deserializer
        _invokeServerConfigurations[configuration.Topic] = configuration;

        // And register the scoped handler within the service provider
        _services.AddScoped(configuration.HandlerType);

        return this;
    }


    /// <summary>
    /// Returns to the parent <see cref="ManagedSignalRConfiguration"/> builder
    /// to allow configuring additional hubs or global settings.
    /// </summary>
    /// <returns>The parent <see cref="ManagedSignalRConfiguration"/> instance for fluent chaining.</returns>
    public ManagedSignalRConfiguration And() => Parent;

}






