using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Implementations;
using ManagedLib.ManagedSignalR.Types;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Hubs;
public class ApplicationHub : ManagedHub
{
    private readonly ManagedHubHelper _helper;

    public ApplicationHub
    (
        ManagedSignalRConfiguration configuration,
        HubCommandDispatcher bus,
        ILogger<ManagedHub> logger,
        IDistributedCacheProvider _cache,
        IDistributedLockProvider _lock,
        ManagedHubHelper helper
    ) : base(configuration, bus, logger, _cache, _lock)
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

        // retrieve the session for the user 
        ManagedHubSession session = await _helper.GetSession(userId);

        // send the alert to every other connection of the user

        foreach (var connection in session.Connections)
        {
            if (connection.ConnectionId != connectionId)
            {
                await _helper.SendToConnectionId<ApplicationHub>(connection.ConnectionId, alert, userId);
            }
        }
    }
}