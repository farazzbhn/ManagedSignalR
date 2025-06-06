using ManagedLib.ManagedSignalR.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;

public class EventBinding
{
    private readonly IServiceCollection _services;

    public EventBinding
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
    /// Maps the Push (outgoing) message types to outgoing topics and serialization functions
    /// </summary>
    public Dictionary<Type, (string Topic, Func<object, string> Serializer)> Outbound { get; set; } = new();

    /// <summary>
    /// Maps the Submit (incoming) topics to handler types and deserialization functions
    /// </summary>
    public Dictionary<string, (Type HandlerType, Type MessageType, Func<string, object> Deserializer)> Inbound { get; set; } = new();

    /// <summary>
    /// Maps a message type to a SignalR topic with custom serialization.
    /// When the server sends a message, it will be serialized using the provided function and routed to clients under the specified topic.
    /// </summary>
    /// <typeparam name="T">The message type to be pushed to clients</typeparam>
    /// <param name="topic">The SignalR topic (or event name) for routing messages</param>
    /// <param name="serializer">serialization function</param>
    public EventBinding OnPush<T>(string topic, Func<T, string> serializer)
    {
        Func<object, string> wrappedSerializer = obj => serializer((T)obj);

        Outbound[typeof(T)] = (topic, wrappedSerializer);
        return this;
    }

    /// <summary>
    /// Maps an incoming topic to a handler type with custom deserialization.
    /// When a message is received on this topic, it will be deserialized and processed by the specified handler.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the message into</typeparam>
    /// <param name="topic">The SignalR topic to listen on</param>
    /// <param name="deserializer">deserialization function</param>
    public EventBinding OnProcess<TModel, THandler>(string topic, Func<string, TModel>? deserializer) where THandler : IHandler<TModel>
    {
        Func<string, object> wrappedDeserializer = json => deserializer(json);

        Inbound[topic] = (typeof(THandler), typeof(TModel), wrappedDeserializer);

        // And register the scoped handler within the service provider
        _services.AddScoped(typeof(THandler));

        return this;
    }
}