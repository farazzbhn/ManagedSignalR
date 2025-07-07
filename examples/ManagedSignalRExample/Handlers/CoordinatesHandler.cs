using ManagedLib.ManagedSignalR.Abstractions;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers.Chat;
public class CoordinatesHandler : IManagedHubHandler<Coordinates>
{
    private readonly ManagedHubHelper<ChatHub> _hubHelper;

    public CoordinatesHandler(ManagedHubHelper<ChatHub> hubHelper)
    {
        _hubHelper = hubHelper;
    }

    public async Task Handle(Coordinates request, HubCallerContext context)
    {
        
    }
}
