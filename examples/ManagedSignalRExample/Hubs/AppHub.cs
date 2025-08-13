using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Hubs;

public class AppHub : ManagedHub
{
    protected override async Task OnConnectedHookAsync()
    {
        var connectionId = Context.ConnectionId;

        // Determine Early or Late group based on current time
        var now = DateTime.Now;
        string timeGroup = now.Hour < 12 ? "EarlyUsers" : "LateUsers";

        // Add user to groups
        await Groups.AddToGroupAsync(connectionId, timeGroup);

        var alert = new Alert()
        {
            Content = $"Welcome! You belong within our {timeGroup} group"
        };

        // Optionally send a welcome message
        await Clients.Caller.TryInvokeClientAsync(alert);
    }

    protected override async Task OnDisconnectedHookAsync()
    {
        var connectionId = Context.ConnectionId;

        // Remove from all possible groups
        await Groups.RemoveFromGroupAsync(connectionId, "EarlyUsers");
        await Groups.RemoveFromGroupAsync(connectionId, "LateUsers");
    }
}