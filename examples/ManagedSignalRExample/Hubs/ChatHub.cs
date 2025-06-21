using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Hubs;
public class ChatHub : ManagedHub
{
    private readonly IHubMediator _mediator;

    public ChatHub
    (
        GlobalConfiguration configuration, 
        ManagedHubHandlerBus bus,
        ILogger<ManagedHub> logger, 
        ICacheProvider cacheProvider, 
        ILockProvider lockProvider
    ) : base(configuration, bus, logger, cacheProvider, lockProvider)
    {
    }

    protected override async Task OnConnectedHookAsync(string userId, string connectionId)
    {

        string[] ids = await _mediator.ListConnectionId<ChatHub>(userId);

        if (ids.Length != 1)
        {
            
            var alert = new Alert()
            {
                Content = "A new device has connected to your account. If this wasn't you, please take immediate action.",
                ActionLabel = "Revoke Access",
                ActionUrl = "https://yourapp.com/security/device"
            };

            foreach (string id in ids)
            {
                if (id != connectionId)
                {
                    var 
                }
            }

        }

    }

}