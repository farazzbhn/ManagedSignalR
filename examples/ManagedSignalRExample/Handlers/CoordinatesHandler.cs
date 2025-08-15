using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers;
public class CoordinatesHandler : IHubCommandHandler<Coordinates>
{
    private readonly IManagedHubContext<AppHub> _hubContext;

    public CoordinatesHandler(IManagedHubContext<AppHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(Coordinates request, HubCallerContext context)
    {

        Console.WriteLine($"User {context.UserIdentifier} is at {request.Latitude}, {request.Longitude}");
        
        var message = new Message
        {
            Text = $"Location received successfully! ({request.Latitude},{request.Longitude})"
        };

        // use IManagedHubContext<> to invoke client
        await _hubContext.Clients.Client(context.ConnectionId).TryInvokeClientAsync(message);
    }
}