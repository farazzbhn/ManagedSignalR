using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Hubs;

public class AppHub : ManagedHub
{
    protected override async Task OnConnectedHookAsync()
    {
        var alert = new Alert
        {
            Content = $"Welcome to the app! Connected as {Context.ConnectionId}",
            ActionLabel = "Get Started",
        };

        await Clients.Caller.TryInvokeClientAsync(alert);
    }

    protected override async Task OnDisconnectedHookAsync()
    {
        var alert = new Alert
        {
            Content = "User left the app"
        };

        await Clients.Others.TryInvokeClientAsync(alert);
    }
}