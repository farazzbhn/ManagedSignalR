using ManagedLib.ManagedSignalR.Configuration;
using ManagedSignalRExample.Handlers;
using ManagedSignalRExample.Hubs;
using ManagedSignalRExample.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5005");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddManagedSignalR(builder =>

        builder
            .AsSingleInstance()

            .AddHub<ApplicationHub>()
                .ConfigureInvokeClient<Alert>(cfg =>
                    cfg
                        .RouteToTopic("alert")
                        // Or do not call UseSerializer to use the default (System.Text.Json) serializer 
                        .UseSerializer(obj => System.Text.Json.JsonSerializer.Serialize(obj)))

                .ConfigureInvokeServer<Coordinates>(cfg =>
                    cfg
                        .FromTopic("loc")
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
);




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();  // ✅ Required before UseAuthorization & MapControllers

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ApplicationHub>("/hub"); // ✅ Maps the API controllers
});

app.Run();
