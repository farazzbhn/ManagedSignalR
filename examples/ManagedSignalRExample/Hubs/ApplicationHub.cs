using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;

namespace ManagedSignalRExample.Hubs;
public class ApplicationHub : ManagedHub<ApplicationHub>
{
    public ApplicationHub(HandlerBus handlerBus, ILogger<ManagedHub<ApplicationHub>> logger, ManagedHubHelper<ApplicationHub> hubHelper, ManagedSignalRConfig configuration) : base(handlerBus, logger, hubHelper, configuration)
    {
    }
}