using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Implementations;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Hubs;
public class MyHub : ManagedHub
{
    protected override async Task OnConnectedHookAsync()
    {

        var alert = new Alert()
        {
            Content = "A new device has connected to your account. If this wasn't you, please take immediate action.",
            ActionLabel = "Revoke Access",
            ActionUrl = "https://yourapp.com/security/device"
        };

        Clients.Client(Context.ConnectionId).TryInvokeClient<MyHub>(alert);

        // retrieve the list of connectionIds associated with the current user
        string[] connectionIds = Connections.UserConnections(Context.UserIdentifier);

        foreach (var id in connectionIds)
        {
            await Helper.SendToConnectionAsync(alert, id);
        }

    }


}