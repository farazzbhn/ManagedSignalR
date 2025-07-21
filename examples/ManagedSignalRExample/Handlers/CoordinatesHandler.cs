using ManagedLib.ManagedSignalR.Abstractions;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers;
public class CoordinatesHandler : IHubCommandHandler<Coordinates>
{
    private readonly ManagedHubHelper _hubHelper;

    public CoordinatesHandler(ManagedHubHelper hubHelper)
    {
        _hubHelper = hubHelper;
    }

    public async Task Handle(Coordinates request, HubCallerContext context, string userId)
    {
        var connectionId = context.ConnectionId;
        var latitude = request.Latitude;
        var longitude = request.Longitude;

        Console.WriteLine($"[CoordinatesHandler] User ID: {userId}, Connection ID: {connectionId}, Coordinates: {latitude}, {longitude}");

        await Task.CompletedTask; // Placeholder for real async work
    }

}
