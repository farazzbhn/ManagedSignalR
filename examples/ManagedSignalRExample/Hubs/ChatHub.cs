using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;

namespace ManagedSignalRExample.Hubs;
public class ChatHub : ManagedHub<ChatHub>
{
    public ChatHub(HandlerBus handlerBus, ILogger<ManagedHub<ChatHub>> logger, ManagedHubHelper<ChatHub> hubHelper, ManagedSignalRConfig configuration) : base(handlerBus, logger, hubHelper, configuration)
    {
    }
}