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
    // --- First Hub ---
    config.AddManagedHub<ApplicationHub>()

        // Route Alert messages to client method with topic "alert", using custom serializer (or default JSON if omitted)
        .ConfigureInvokeClient<Alert>(cfg =>
            cfg
                .RouteToTopic("alert")
                .UseSerializer(obj => System.Text.Json.JsonSerializer.Serialize(obj)))

        // Handle Coordinates messages from client topic "loc" using custom deserializer and CoordinatesHandler
        .ConfigureInvokeServer<Coordinates>(cfg =>
            cfg
                .OnTopic("loc")
                // coordinates are sent as "lat,long" string, so we need to parse it
                .UseDeserializer(str =>
                {
                    var split = str.Split(',');
                    return new Coordinates
                    {
                        Latitude = double.Parse(split[0]),
                        Longitude = double.Parse(split[1])
                    };
                })
                // Use CoordinatesHandler to process the coordinates
                .UseHandler<CoordinatesHandler>())


        // Route Message objects with default System.Text.Json serializer (here hardcoded for demo)
        .ConfigureInvokeClient<Message>(cfg =>
            cfg
                .UseSerializer(str => "sda"));


    // --- Second Hub ---
    config.AddManagedHub<ChatHub>()
        // Route ChatMessage to client with topic "chat-msg" using custom serializer
        .ConfigureInvokeClient<ChatMessage>(cfg =>
            cfg
                .RouteToTopic("chat-msg")
                .UseSerializer(obj => System.Text.Json.JsonSerializer.Serialize(obj)))

        // Handle TypingStatus messages from topic "typing" using custom deserializer and TypingStatusHandler
        .ConfigureInvokeServer<TypingStatus>(cfg =>
            cfg
                .OnTopic("typing")
                .UseDeserializer(str => new TypingStatus { IsTyping = bool.Parse(str) })
                .UseHandler<TypingStatusHandler>());
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
    endpoints.MapHub<ApplicationHub>("/myhub");          // ✅ Maps the API controllers
});

app.Run();
