namespace ManagedLib.ManagedSignalR.Core;


public record UserConnection(string ConnectionId, string InstanceId);


internal class UserConnectionGroup
{
    private readonly HashSet<UserConnection> _connections = new();

    public IReadOnlyCollection<UserConnection> Connections => _connections;


    /// <summary>
    /// Ensures that a new <see cref="UserConnectionGroup"/> is created with a valid connection ID and instance ID.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="instanceId"></param>
    /// <exception cref="ArgumentException"></exception>
    internal UserConnectionGroup(string connectionId, string instanceId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentException("ConnectionId cannot be null or empty.", nameof(connectionId));
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException("InstanceId cannot be null or empty.", nameof(instanceId));

        AddConnection(connectionId, instanceId);
    }

    public void AddConnection(string connectionId, string instanceId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentException("ConnectionId cannot be null or empty.", nameof(connectionId));
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException("InstanceId cannot be null or empty.", nameof(instanceId));

        var connection = new UserConnection(connectionId, instanceId);
        lock (_connections)
        {
            _connections.Add(connection);
        }
    }

    public bool RemoveConnection(string connectionId)
    {
        lock (_connections)
        {
            var connectionToRemove = _connections.FirstOrDefault(c => c.ConnectionId == connectionId);
            if (connectionToRemove != null)
            {
                return _connections.Remove(connectionToRemove);
            }
            return false;
        }
    }
}
