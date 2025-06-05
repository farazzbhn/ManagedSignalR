using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Configuration;


public class EventMapping
{
    public EventMapping(Type hub)
    {
        Hub = hub;
    }

    /// <summary>
    /// Concrete  Hub type
    /// </summary>
    public Type Hub { get; set; }

    /// <summary>
    /// Maps the Push (outgoing) message types to outgoing topics
    /// </summary>
    public Dictionary<Type, string> Outgoing { get; set; } = new();

    /// <summary>
    /// Maps the Submit (incoming) topics to MediatR IRequest implementations
    /// </summary>
    public Dictionary<Type, string> Incoming { get; set; } = new();

    public EventMapping OnSubmit<TCommand>(string topic)
    {
        Incoming[typeof(TCommand)] = topic;
        return this;
    }

    /// <summary>
    /// Maps a message type inheriting from <see cref="IPushNotification"/> to a SignalR topic. <br />
    /// When the server sends a message, the message of type <typeparamref name="T"/> is serialized and routed to clients under the specified topic. <br />
    /// This enables type-safe, topic-based message routing between server and clients.
    /// </summary>
    /// <typeparam name="T">
    /// The message type that must inherit from <see cref="IPushNotification"/>.
    /// </typeparam>
    /// <param name="topic">
    /// The SignalR topic (or event name) for routing messages of type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="EventMapping"/>, allowing method chaining.
    /// </returns>
    public EventMapping PushNotification<T>(string topic) where T : IPushNotification
    {
        Outgoing[typeof(T)] = topic;
        return this;
    }

}