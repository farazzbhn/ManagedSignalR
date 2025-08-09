using ManagedSignalRClientExample;
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Hello, World!");

// Build the connection
var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5005/myhub")
    .WithAutomaticReconnect()
    .Build();

// Handle server-initiated messages via InvokeClient(topic, payload)
connection.On<string, string>("InvokeClient", (topic, payload) =>
{
    switch (topic)
    {
        case "alert":
            var alert = System.Text.Json.JsonSerializer.Deserialize<Alert>(payload);
            Console.WriteLine($"[alert] Message: {alert?.Content}");
            break;


        case "msg":
            var msg = System.Text.Json.JsonSerializer.Deserialize<Message>(payload);
            Console.WriteLine($"[message] Message: {msg?.Text}");
            break;

        default:
            Console.WriteLine($"[unknown topic] {topic} => {payload}");
            break;
    }
});

// Wait for user confirmation before connecting
Console.WriteLine("Press ENTER to connect to the hub...");
Console.ReadLine();

try
{
    await connection.StartAsync();
    Console.WriteLine("Connected to hub.");
}
catch (Exception ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
    return;
}

// Main interaction loop
while (true)
{
    Console.WriteLine("Enter coordinates as lat,long (or 'exit'):");
    var input = Console.ReadLine();
    if (input == "exit") break;

    try
    {
        await connection.SendAsync("InvokeServer", "loc", input); // Topic-based send
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send coordinates: {ex.Message}");
    }
}

await connection.StopAsync();
Console.WriteLine("Disconnected.");