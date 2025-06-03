namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Base class for messages that are routed through SignalR using topic-based routing. <br/><br/>
/// </summary>
public abstract class TopicMessage
{
    /// <summary>
    /// Serializes the message to JSON format for transport through SignalR.
    /// Override this method if you need custom serialization behavior.
    /// </summary>
    /// <returns>JSON string representation of the message.</returns>
    public virtual string ToText() => System.Text.Json.JsonSerializer.Serialize(this, GetType());
}
