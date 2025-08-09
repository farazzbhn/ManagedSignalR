using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Implementations;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Hubs;
public class MyHub : AbstractManagedHub
{
    protected override async Task OnConnectedHookAsync()
    {

        var alert = new Alert()
        {
            Content = "A new device has connected to your account. If this wasn't you, please take immediate action.",
            ActionLabel = "Revoke Access",
            ActionUrl = "https://yourapp.com/security/device"
        };

        // retrieve the list of connectionIds associated with the current user
        string[] connectionIds = await Connections.ListConnectionIdsAsync(Context.UserIdentifier);

        // send the alert to every other connection of the user



        foreach (var id in connectionIds)
        {
            await Helper.SendToConnectionIdAsync(alert, id);
        }
    }


}