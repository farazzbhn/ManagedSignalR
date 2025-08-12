using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Implementations;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Hubs;
public class AppHub : ManagedHub
{
    protected override async Task OnConnectedHookAsync()
    {

        var alert = new Alert()
        {
            Content = "A new device has connected to your account. If this wasn't you, please take immediate action.",
            ActionLabel = "Revoke Access",
            ActionUrl = "https://yourapp.com/security/device"
        };

        await Clients.Client(Context.ConnectionId).TryInvokeClientAsync(alert);
    }

}