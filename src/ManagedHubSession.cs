namespace ManagedLib.ManagedSignalR;


/// <summary>
/// Represents a userId-specific SignalR hub connection, including a collection of active connection IDs.
/// </summary>
public class ManagedHubSession
{
    public string UserId { get; set; } = default!;
    public List<string> ConnectionIds { get; set; } = new();
}
