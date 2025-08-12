using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Core;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers;
public class CoordinatesHandler : IHubCommandHandler<Coordinates>
{
    private readonly IManagedHubContext<ApplicationHub> _hubContext;

    public CoordinatesHandler(IManagedHubContext<ApplicationHub> hubContext)
    {
        _hubContext = hubContext;
    }


    public async Task Handle(Coordinates request, HubCallerContext context)
    {
        var connectionId = context.ConnectionId;
        var latitude = request.Latitude;
        var longitude = request.Longitude;

        Console.WriteLine(
            $"{nameof(CoordinatesHandler)}\n" +
            $"\tUser : {context.UserIdentifier ?? string.Empty}\n" +
            $"\tConnection ID: {connectionId}\n" +
            $"\tCoordinates: {latitude}, {longitude}" +
            $"\n\n"
        );


        await _hubContext.ManagedClients.Client(connectionId).InvokeClientAsync(new Message() { Text = $"your location is {latitude} , {longitude}" });

        await Task.CompletedTask; 
    }
}
