using ManagedLib.ManagedSignalR.Abstractions;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers;
public class CoordinatesHandler : IHubCommandHandler<Coordinates>
{
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

        await Task.CompletedTask; // Placeholder for real async work
    }
}
