using ManagedSignalRClientExample;
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Hello, World!");

// Build the connection
var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5005/apphub")
    .WithAutomaticReconnect()
    .Build();


// Handle server-initiated messages via InvokeClient(topic, payload)
connection.On<string, string>("InvokeClient", (topic, payload) =>
{
    switch (topic)
    {
        case "alert":
            var alert = System.Text.Json.JsonSerializer.Deserialize<Alert>(payload);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ALERT!!!\t{alert?.Content}");
            Console.ResetColor();
            break;

        case "msg":
            var msg = System.Text.Json.JsonSerializer.Deserialize<Message>(payload);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"NEW MESSAGE*\t{msg?.Text}");
            Console.ResetColor();
            break;

        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[unexpected topic]\t{topic} => {payload}");
            Console.ResetColor();
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
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Enter coordinates as lat,long (or 'exit'):");
    Console.ResetColor();

    var input = Console.ReadLine();
    if (input == "exit") break;

    try
    {
        await connection.SendAsync("InvokeServer", "gps", input); // Topic-based send
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send coordinates: {ex.Message}");
    }
}

await connection.StopAsync();
Console.WriteLine("Disconnected.");