using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers;
public class NewMessageHandler : IManagedHubHandler<NewMessage>
{

    private readonly ManagedHubHelper<ApplicationHub> _hubHelper;
    private readonly IIdentityProvider _identityProvider;

    public NewMessageHandler
    (
        ManagedHubHelper<ApplicationHub> hubHelper, 
        IIdentityProvider identityProvider
    )
    {
        _hubHelper = hubHelper;
        _identityProvider = identityProvider;
    }

    public async Task Handle(NewMessage request, HubCallerContext context)
    {

        string userId = _identityProvider.GetUserId(context);

        var msg = new Message()
        {
            Id = Guid.NewGuid().ToString(),
            Role = Role.System,
            Text = $"You said : {request.Text}"
        };

        await _hubHelper.PushToClient(userId, msg);
    }
}
