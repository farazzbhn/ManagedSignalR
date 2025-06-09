using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers;
public class TextMessageHandler : IManagedHubHandler<TextMessage>
{

    private readonly ManagedHubHelper<ChatHub> _hubHelper;

    public TextMessageHandler(ManagedHubHelper<ChatHub> hubHelper)
    {
        _hubHelper = hubHelper;
    }

    public async Task Handle(TextMessage request, HubCallerContext context)
    {
        await _hubHelper.TrySendToClient();
    }
}
