using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Hubs;
public class ApplicationHub : ManagedHub
{
    private readonly ManagedHubHelper _helper;

    public ApplicationHub(
        ManagedSignalRConfiguration globalConfiguration, 
        ILogger<ManagedHub> logger, 
        ICacheProvider cacheProvider, 
        IServiceProvider serviceProvider,
        ManagedHubHelper helper
    ) : base(globalConfiguration, logger, cacheProvider, serviceProvider)
    {
        _helper = helper;
    }

    protected override async Task OnConnectedHookAsync(string userId)
    {
        Console.WriteLine($"The {nameof(ApplicationHub)} method \"OnConnectedHookAsync\" is invoked AFTER every client Connection!");

        var alert = new Alert()
        {
            Content = "A new device has connected to your account. If this wasn't you, please take immediate action.",
            ActionLabel = "Revoke Access",
            ActionUrl = "https://yourapp.com/security/device"
        };

        // retrieve the session for the user 
        await _helper.SendToConnectionId<ApplicationHub>(Context.ConnectionId, alert);
    }


    protected override async Task OnDisconnectedHookAsync(string userId)
    {
        Console.WriteLine($"The {nameof(ApplicationHub)} method \"OnDisconnectedHookAsync\" is invoked AFTER every client disconnection!");
    }
}