using System.ComponentModel;
using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ManagedLib.ManagedSignalR.Configuration;


public sealed class ManagedHubConfiguration
{
    private readonly IServiceCollection _services;

    public ManagedHubConfiguration
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


    /// <summary>
    /// Maps incoming topics to handlers and deserializers
    /// </summary>
    internal Dictionary<string, (Type HandlerType, Type MessageType, Func<string, object> Deserializer)> ReceiveConfig { get; set; } = new();


    /// <summary>
    /// Maps outgoing message types to topics and serializers
    /// </summary>
    internal Dictionary<Type, (string Topic, Func<object, string> Serializer)> Client { get; set; } = new();


    /// <summary>
    /// Configures how messages are sent to clients
    /// </summary>
    /// <typeparam name="T">Message type to send</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public ManagedHubConfiguration ConfigReceiveOnClient<T>(Action<SendToClientConfiguration<T>> configurer)
    {
        
        var configuration = new SendToClientConfiguration<T>();

        configurer.Invoke(configuration);


        // Validation
        InvalidEnumArgumentException.ThrowIfNullOrEmpty(configuration.Topic);

        // and store 

        Client[typeof(T)] = (configuration.Topic, obj => configuration.Serializer((T)obj));

        return this;
    }

    /// <summary>
    /// Configures how messages are received from clients
    /// </summary>
    /// <typeparam name="TModel">Message type to receive</typeparam>
    /// <param name="configurer">Configuration builder</param>
    public ManagedHubConfiguration ConfigReceiveOnServer<TModel>(Action<ReceiveOnServerConfiguration<TModel>> configurer)
    {

        var configuration = new ReceiveOnServerConfiguration<TModel>();

        configurer.Invoke(configuration);
        
        // Throw if topic is not defined
        if (configuration.HandlerType is null) throw new ArgumentException("Handler not specified!");
        ArgumentException.ThrowIfNullOrEmpty(configuration.Topic);


        ReceiveConfig[configuration.Topic] = (configuration.HandlerType, typeof(TModel), json => configuration.Deserializer(json));

        // And register the scoped handler within the service provider
        _services.AddScoped(configuration.HandlerType);

        return this;
    }
}



public sealed class ReceiveOnServerConfiguration<TModel>
{
    internal string Topic { get; private set; }

    internal Func<string, TModel> Deserializer { get; private set; } = message => System.Text.Json.JsonSerializer.Deserialize<TModel>(message)!;

    internal Type HandlerType { get; private set; } = null;

    /// <summary>
    /// Sets the topic for incoming messages
    /// </summary>
    public ReceiveOnServerConfiguration<TModel> BindTopic(string topic)
    {
        Topic = topic;
        return this;
    }


    /// <summary>
    /// Sets custom message deserialization
    /// </summary>
    public ReceiveOnServerConfiguration<TModel> UseDeserializer(Func<string, TModel> deserializer)
    {
        this.Deserializer = deserializer;
        return this;
    }

    /// <summary>
    /// Sets the handler type for processing messages
    /// </summary>
    public ReceiveOnServerConfiguration<TModel> UseHandler<THandler>() where THandler : IManagedHubHandler<TModel>
    {
        HandlerType = typeof(THandler);
        return this;
    }

}


public sealed class SendToClientConfiguration<TModel>
{
    internal string? Topic { get; private set; }
    internal Func<TModel, string> Serializer { get; private set; } = message => System.Text.Json.JsonSerializer.Serialize(message);

    /// <summary>
    /// Sets the topic for outgoing messages
    /// </summary>
    public SendToClientConfiguration<TModel> BindTopic(string topic)
    {
        Topic = topic;
        return this;
    }

    /// <summary>
    /// Sets custom message serialization
    /// </summary>
    public SendToClientConfiguration<TModel> UseSerializer(Func<TModel, string> serializer)
    {
        Serializer = serializer;
        return this;
    }
}