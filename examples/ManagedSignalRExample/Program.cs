using ManagedLib.ManagedSignalR.Configuration;
using ManagedSignalRExample.Handlers;
using ManagedSignalRExample.Handlers.Chat;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddManagedSignalR(config =>
{
    config.AddHub<ChatHub>()

        .OnInvokeClient<Message>(cfg =>
            cfg
                .BindTopic("msg")
                .UseSerializer(obj => System.Text.Json.JsonSerializer.Serialize(obj)))

        .OnInvokeServer<Coordinates>(cfg =>
            cfg
                .BindTopic("loc")
                // coordinates are sent as "lat,long" string 
                .UseDeserializer(str =>
                {
                    string[] split = str.Split(',');
                    return new Coordinates
                    {
                        Latitude = double.Parse(split[0]),
                        Longitude = double.Parse(split[1])
                    };
                })
                .UseHandler<CoordinatesHandler>())

        .OnInvokeServer<NewMessage>(cfg =>
            cfg
                .BindTopic("msg")
                // Not invoking the UseDeserializer method here means that the incoming message
                // will be deserialized using the default JSON deserializer.
                //.UseDeserializer(str => System.Text.Json.JsonSerializer.Deserialize<IncomingMessage>(str))
                .UseHandler<NewMessageHandler>());

});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
