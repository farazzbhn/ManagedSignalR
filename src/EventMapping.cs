using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR;


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
    /// Maps the Whisper (outgoing) message types to outgoing topics
    /// </summary>
    public Dictionary<Type, string> Outgoing { get; set; } = new();

    /// <summary>
    /// Maps the Process (incoming) topics to MediatR IRequest implementations
    /// </summary>
    public Dictionary<Type, string> Incoming { get; set; } = new();

    /// <summary>
    /// Maps a SignalR topic in <see cref="ManagedHub{T}.Process"/> to a corresponding <see cref="ICommand{TResponse}"/> type. <br />
    /// When the client invokes <see cref="ManagedHub{T}.Process"/> on the server (via the SignalR client library), 
    /// the message body is automatically deserialized into an instance of <typeparamref name="T"/> and dispatched through the <see cref="ICommandBus"/>. <br />
    /// This provides a structured and type-safe integration between SignalR messages and the command handling pipeline.
    /// </summary>
    /// <typeparam name="T">
    /// The command type, which must implement <see cref="ICommand{TResponse}"/>.
    /// </typeparam>
    /// <param name="topic">
    /// The SignalR topic (or event name) that triggers the command processing.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="EventMapping"/>, enabling method chaining.
    /// </returns>
    public EventMapping BindProcessTopicToRequest<T>(string topic) where T : ICommand<None>
    {
        Incoming[typeof(T)] = topic;
        return this;
    }

    /// <summary>
    /// Maps a message type inheriting from <see cref="TopicMessage"/> to a SignalR topic. <br />
    /// When the server sends a message, the message of type <typeparamref name="T"/> is serialized and routed to clients under the specified topic. <br />
    /// This enables type-safe, topic-based message routing between server and clients.
    /// </summary>
    /// <typeparam name="T">
    /// The message type that must inherit from <see cref="TopicMessage"/>.
    /// </typeparam>
    /// <param name="topic">
    /// The SignalR topic (or event name) for routing messages of type <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// The current instance of <see cref="EventMapping"/>, allowing method chaining.
    /// </returns>
    public EventMapping BindTypeToWhisperTopic<T>(string topic) where T : TopicMessage
    {
        Outgoing[typeof(T)] = topic;
        return this;
    }

}