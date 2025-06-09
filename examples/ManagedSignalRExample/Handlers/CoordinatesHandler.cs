using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers.Chat;
public class CoordinatesHandler : IManagedHubHandler<Coordinates>
{
    private readonly ManagedHubHelper<ApplicationHub> _hubHelper;

    public CoordinatesHandler(ManagedHubHelper<ApplicationHub> hubHelper)
    {
        _hubHelper = hubHelper;
    }

    public async Task Handle(Coordinates request, HubCallerContext context)
    {
        
    }
}
