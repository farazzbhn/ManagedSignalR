using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;


/// <summary>
/// Holds mappings and other configuration options for a specific SignalR hub endpoint.
/// </summary>
public sealed class EndpointOptions
{
    internal IServiceCollection? Services { get; set; }
    public readonly ManagedSignalROptions Parent;

    public EndpointOptions
    (
        Type hubType,
        ManagedSignalROptions parent,
        IServiceCollection services
    )
    {
        HubType = hubType;
        Parent = parent;
        Services = services;
        _invokeServerConfigurations = new();
        _invokeClientConfigurations = new();
    }

    /// <summary>
    /// Finalize the object &amp; clear the unnecessary references
    /// </summary>
    internal void Seal() => Services = null;


    /// <summary>
    /// Hub type being configured
    /// </summary>
    internal Type HubType { get; set; }

    private Dictionary<string, InvokeServerConfiguration> _invokeServerConfigurations { get; set; }

    private Dictionary<Type, InvokeClientConfiguration> _invokeClientConfigurations { get; set; }

    internal IReadOnlyDictionary<string, InvokeServerConfiguration> InvokeServerConfigurations => _invokeServerConfigurations;
    internal IReadOnlyDictionary<Type, InvokeClientConfiguration> InvokeClientConfigurations => _invokeClientConfigurations;


    /// <summary>
    /// Configures how messages are sent to clients
    /// </summary>
    /// <typeparam name="TOutboundMessage">Message type to send</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public EndpointOptions ConfigureInvokeClient<TOutboundMessage>(Action<InvokeClientConfiguration<TOutboundMessage>> configurer)
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
    public EndpointOptions ConfigureInvokeServer<TInboundMessage>(Action<InvokeServerConfiguration<TInboundMessage>> configurer)
    {

        var configuration = new InvokeServerConfiguration<TInboundMessage>();

        configurer.Invoke(configuration);

        configuration.EnsureIsValid();

        // a string (topic) is bound to a  C# type/deserializer
        _invokeServerConfigurations[configuration.Topic] = configuration;

        // And register the scoped handler within the service provider
        Services.AddScoped(configuration.HandlerType);

        return this;
    }


    /// <summary>
    /// Returns to the parent <see cref="ManagedSignalROptions"/> builder
    /// to allow configuring additional hubs or global settings.
    /// </summary>
    /// <returns>The parent <see cref="ManagedSignalROptions"/> instance for fluent chaining.</returns>
    public ManagedSignalROptions And() => Parent;

}






