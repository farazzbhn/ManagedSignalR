using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;

namespace ManagedSignalRExample.Hubs;
public class ChatHub : AbstractManagedHub
{
    private readonly ManagedHubHelper _helper;

    public ChatHub
    (
        ManagedSignalRConfiguration globalConfiguration, 
        ILogger<AbstractManagedHub> logger, 
        IDistributedCache cacheProvider, 
        IServiceProvider serviceProvider,
        ManagedHubHelper helper
    ) : base(globalConfiguration, logger, cacheProvider, serviceProvider)
    {
        _helper = helper;
    }
    protected sealed override async Task OnConnectedHookAsync(string userId)
    {
        Console.WriteLine($"The {nameof(ChatHub)} method \"OnConnectedHookAsync\" is invoked AFTER every client Connection!");
    }


    protected sealed override async Task OnDisconnectedHookAsync(string userId)
    {
        Console.WriteLine($"The {nameof(ChatHub)} method \"OnDisconnectedHookAsync\" is invoked AFTER every client disconnection!");
    }
}