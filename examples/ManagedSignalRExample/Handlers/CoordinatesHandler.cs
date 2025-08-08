using ManagedLib.ManagedSignalR.Abstractions;
using ManagedSignalRExample.Models;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Handlers;
public class CoordinatesHandler : IHubCommandHandler<Coordinates>
{
    private readonly ManagedHubHelperUtils _hubHelper;

    public CoordinatesHandler(ManagedHubHelperUtils hubHelper)
    {
        _hubHelper = hubHelper;
    }

    public async Task Handle(Coordinates request, HubCallerContext context, string userId)
    {
        var connectionId = context.ConnectionId;
        var latitude = request.Latitude;
        var longitude = request.Longitude;

        //Console.WriteLine(
        //    $"{nameof(CoordinatesHandler)}\n" +
        //    $"\tUser : {userId}\n" +
        //    $"\tConnection ID: {connectionId}\n" +
        //    $"\tCoordinates: {latitude}, {longitude}"+
        //    $"\n\n" 
        //);

        await Task.CompletedTask; // Placeholder for real async work
    }

}
