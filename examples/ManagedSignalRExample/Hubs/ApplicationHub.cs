using ManagedLib.ManagedSignalR;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Implementations;

namespace ManagedSignalRExample.Hubs;
public class ApplicationHub : ManagedHub
{
    public ApplicationHub(
        GlobalSettings settings, ManagedHubHandlerBus bus, ILogger<ManagedHub> logger, ICacheProvider cacheProvider, DefaultLockProvider lockProvider) : base(settings, bus, logger, cacheProvider, lockProvider)
    {
    }
}