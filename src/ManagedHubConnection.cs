using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR;

/// <summary>
/// Represents a userId-specific SignalR hub connection, including a collection of active connection IDs.
/// </summary>
public class ManagedHubConnection<T>
{
    public string UserId { get; set; } = default!;
    public List<string> ConnectionIds { get; set; } = new();

}
