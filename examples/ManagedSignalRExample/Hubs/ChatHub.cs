using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Hubs;
public class ChatHub : ManagedHub
{
    private readonly IManagedHubHelper _helper;

    public ChatHub
    (
        GlobalConfiguration configuration,
        ManagedHubHandlerBus bus,
        ILogger<ManagedHub> logger,
        ICacheProvider cacheProvider,
        ILockProvider lockProvider,
        IManagedHubHelper helper
    ) : base(configuration, bus, logger, cacheProvider, lockProvider)
    {
        _helper = helper;
    }

    protected override async Task OnConnectedHookAsync(string userId, string connectionId)
    {

        var alert = new Alert()
        {
            Content = "A new device has connected to your account. If this wasn't you, please take immediate action.",
            ActionLabel = "Revoke Access",
            ActionUrl = "https://yourapp.com/security/device"
        };


        await _helper.InvokeClient<ChatHub>(alert, userId);

    }
}