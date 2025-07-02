namespace ManagedLib.ManagedSignalR.Abstractions;


/// <summary>
/// Represents a userId-specific SignalR hub connection, including a collection of active connection IDs.
/// </summary>
public class ManagedHubSession
{
    public string UserId { get; set; } = default!;
    public List<Connection> Connections { get; set; } = new();
}


public class Connection
{
    public string ConnectionId { get; set; }
    public string InstanceId { get; set; }

    public Connection(string instanceId, string connectionId)
    {
        InstanceId = instanceId;
        ConnectionId = connectionId;
    }
}