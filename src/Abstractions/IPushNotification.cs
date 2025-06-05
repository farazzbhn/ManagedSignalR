namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// The contract representing the payload sent via SignalR using topic-based routing. <br />
/// Used as the payload for the <see cref="IClient.Push"/> method, which accepts a topic and this payload.
/// </summary>
public interface  IPushNotification
{
    /// <summary>
    /// Converts the object to a string representation suitable for transport via the
    /// <see cref="IClient.Push"/> SignalR method. <br />
    /// The format may vary (e.g., JSON, plain text).
    /// </summary>
    /// <returns>A string representation of the message body.</returns>
    public string ToPayload();
}
