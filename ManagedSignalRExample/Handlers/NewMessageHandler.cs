using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;

namespace ManagedSignalRExample.Handlers;
public class NewMessageHandler : IManagedHubHandler<UserMessage>
{

    private readonly ManagedHubHelper<ChatHub> _hubHelper;

    public NewMessageHandler(ManagedHubHelper<ChatHub> hubHelper)
    {
        _hubHelper = hubHelper;
    }

    public async Task Handle(UserMessage request)
    {
    }
}
