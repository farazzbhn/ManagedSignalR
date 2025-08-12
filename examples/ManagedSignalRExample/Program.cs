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

        config.AddManagedHub<MyHub>()
            // Messages of type Alert are routed to client-side method InvokeClient("alert", "{...}") with topic:alert
            // and serialized using the below serializer ( or default System.Text.Json if not available ) 
            .ConfigureInvokeClient<Alert>(cfg =>
                cfg
                    .RouteToTopic("alert")
                    // Or do not call UseSerializer to use the default (System.Text.Json) serializer 
                    .UseSerializer(obj => System.Text.Json.JsonSerializer.Serialize(obj)))

            // Messages received over method InvokeServer(topic:"loc", message:"100,200") are deserialized into 
            // "Coordinates" using the custom deserializer and handled via a scoped instance of "CoordinatesHandler"
            .ConfigureInvokeServer<Coordinates>(cfg =>
                cfg
                    .OnTopic("loc")
                    // coordinates are expected as "lat,long" string : 100,200
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

            // use the default  default (System.Text.Json) serializer 
            .ConfigureInvokeClient<Message>(cfg => 
                cfg
                    .UseSerializer(str => @"sda")
            );

        // signalR configuration
        config
            .WithKeepAliveInterval(30)
            .WithEnabledDetailedErrors()
            .WithSupportedProtocols("json");
    }
);




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
    endpoints.MapHub<MyHub>("/myhub");          // ✅ Maps the API controllers
});

app.Run();
