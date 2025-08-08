namespace ManagedLib.ManagedSignalR.Types;


internal class ConnectionSet
{
    private readonly HashSet<string> _connections = new();

    public IReadOnlyCollection<string> Connections => _connections;


    /// <summary>
    /// Ensures that a new <see cref="ConnectionSet"/> is created with a valid connection ID.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <exception cref="ArgumentException"></exception>
    internal ConnectionSet(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentException("ConnectionId cannot be null or empty.", nameof(connectionId));

        AddConnection(connectionId);
    }



    public void AddConnection(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentException("ConnectionId cannot be null or empty.", nameof(connectionId));

        lock (_connections)
        {
            _connections.Add(connectionId);
        }
    }

    public bool RemoveConnection(string connectionId)
    {
        lock (_connections)
        {
            string? connectionToRemove = _connections.FirstOrDefault(x => x == connectionId);

            if (connectionToRemove != null)
            {
                return _connections.Remove(connectionToRemove);
            }

            return false;
        }
    }
}
