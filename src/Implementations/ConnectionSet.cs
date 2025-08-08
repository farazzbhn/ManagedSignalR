namespace ManagedLib.ManagedSignalR.Implementations;


internal class ConnectionSet
{

    private readonly HashSet<string> _connectionIds = new();

    public IReadOnlyCollection<string> ConnectionIds => _connectionIds;

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

        lock (_connectionIds)
        {
            _connectionIds.Add(connectionId);
        }
    }

    public bool RemoveConnection(string connectionId)
    {
        lock (_connectionIds)
        {
            string? connectionToRemove = _connectionIds.FirstOrDefault(x => x == connectionId);

            if (connectionToRemove != null)
            {
                return _connectionIds.Remove(connectionToRemove);
            }

            return false;
        }
    }
}
