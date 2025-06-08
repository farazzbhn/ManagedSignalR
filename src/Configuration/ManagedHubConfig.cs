using System.ComponentModel;
using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ManagedLib.ManagedSignalR.Configuration;

public class ManagedHubConfig
{
    private readonly IServiceCollection _services;

    public ManagedHubConfig
    (
        Type hubType,
        IServiceCollection services
    )
    {
        _services = services;
        HubType = hubType;
    }


    /// <summary>
    /// Concrete Hub type
    /// </summary>
    public Type HubType { get; set; }


    /// <summary>
    /// Maps the Submit (incoming) topics to handler types and deserialization functions
    /// </summary>
    internal Dictionary<string, (Type HandlerType, Type MessageType, Func<string, object> Deserializer)> Inbound { get; set; } = new();


    /// <summary>
    /// Maps the Push (outgoing) message types to outgoing topics and serialization functions
    /// </summary>
    internal Dictionary<Type, (string Topic, Func<object, string> Serializer)> Outbound { get; set; } = new();


    /// <summary>
    /// Maps a message type to a SignalR topic with custom serialization.
    /// When the server sends a message, it will be serialized using the provided function and routed to clients under the specified topic.
    /// </summary>
    /// <typeparam name="T">The message type to be pushed to clients</typeparam>
    public ManagedHubConfig ConfigureNotifyClient<T>(Action<NotifyClientConfiguration<T>> configurer)
    {
        
        var configuration = new NotifyClientConfiguration<T>();

        configurer.Invoke(configuration);


        // Validation
        InvalidEnumArgumentException.ThrowIfNullOrEmpty(configuration.Topic);

        // and store 

        Outbound[typeof(T)] = (configuration.Topic, obj => configuration.Serializer((T)obj));

        return this;
    }

    /// <summary>
    /// Maps an incoming topic to a C# type processed using the dedicated handler, and deserialized using the custom deserialization provided.
    /// <b>Registers the handler within the service provider</b>
    /// </summary>
    public ManagedHubConfig ConfigureNotifyServer<TModel>(Action<NotifyServerConfiguration<TModel>> configurer)
    {

        var configuration = new NotifyServerConfiguration<TModel>();

        configurer.Invoke(configuration);
        
        // Throw if topic is not defined
        if (configuration.HandlerType is null) throw new ArgumentException("Handler not specified!");
        ArgumentException.ThrowIfNullOrEmpty(configuration.Topic);


        Inbound[configuration.Topic] = (configuration.HandlerType, typeof(TModel), json => configuration.Deserializer(json));

        // And register the scoped handler within the service provider
        _services.AddScoped(configuration.HandlerType);

        return this;
    }
}
    

public class NotifyServerConfiguration<TModel>
{
    internal string Topic { get; private set; }

    internal Func<string, TModel> Deserializer { get; private set; } = message => System.Text.Json.JsonSerializer.Deserialize<TModel>(message)!;

    internal Type HandlerType { get; private set; } = null;

    public NotifyServerConfiguration<TModel> OnTopic(string topic)
    {
        Topic = topic;
        return this;
    }


    public NotifyServerConfiguration<TModel> UseDeserializer(Func<string, TModel> deserializer)
    {
        this.Deserializer = deserializer;
        return this;
    }

    public NotifyServerConfiguration<TModel> UseHandler<THandler>() where THandler : IManagedHubHandler<TModel>
    {
        HandlerType = typeof(THandler);
        return this;
    }

}


public class NotifyClientConfiguration<TModel>
{
    internal string? Topic { get; private set; }
    internal Func<TModel, string> Serializer { get; private set; } = message => System.Text.Json.JsonSerializer.Serialize(message);

    public NotifyClientConfiguration<TModel> ToTopic(string topic)
    {
        Topic = topic;
        return this;
    }

    public NotifyClientConfiguration<TModel> UseSerializer(Func<TModel, string> serializer)
    {
        Serializer = serializer;
        return this;
    }
}