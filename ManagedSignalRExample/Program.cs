using ManagedLib.ManagedSignalR.Configuration;
using ManagedSignalRExample.Handlers;
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
    config.AddHub<ApplicationHub>()

        .ConfigureNotifyClient<MessageModel>(config => config
            .BindTopic("msg")
            .UseSerializer(obj => System.Text.Json.JsonSerializer.Serialize(obj)))

        .ConfigureNotifyServer<UserMessage>(config =>
            config
                .BindTopic("msg")
                .UseDeserializer(obj => System.Text.Json.JsonSerializer.Deserialize<UserMessage>(obj))
                .UseHandler<NewMessageHandler>())

        .ConfigureNotifyServer<UserMessage>(config =>
            config
                .BindTopic("msg")
                .UseDeserializer(obj => System.Text.Json.JsonSerializer.Deserialize<UserMessage>(obj))
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
