using System.Text.Json;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedSignalRExample.Handlers;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5005");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddManagedSignalR(config =>
{
    /* FIRST HUB */
    config.AddManagedHub<AppHub>()

        // Configure outgoing messages (server to client)
        .ConfigureInvokeClient<Alert>(cfg =>
            cfg.RouteToTopic("alert")
                .UseSerializer(obj => JsonSerializer.Serialize(obj)))


        // Configure incoming messages (client to server)  
        .ConfigureInvokeServer<Coordinates>(cfg =>
            cfg.OnTopic("gps")
                // coordinates are received as "lat,long"
                .UseDeserializer(str =>
                {
                    var parts = str.Split(',');
                    return new Coordinates
                    {
                        Latitude = double.Parse(parts[0]), // assuming the 1st part is latitude
                        Longitude = double.Parse(parts[1]) // assuming the 2nd part is longitude
                    };
                })
                .UseHandler<CoordinatesHandler>())


        .ConfigureInvokeClient<Message>(cfg =>
            // Configure outgoing messages (server to client) for "message" topic
            // do not specify a serializer, it will use the default JSON serializer
            cfg.RouteToTopic("msg"));


    /* SECOND HUB */
    //config.AddManagedHub<ChatHub>()...

});




builder.Services.AddSignalR(); // ✅ Required for SignalR Hubs


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();                                       // ✅ Required before UseAuthorization & MapControllers

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<AppHub>("/apphub");          // ✅ Maps the API controllers
});

app.Run();
