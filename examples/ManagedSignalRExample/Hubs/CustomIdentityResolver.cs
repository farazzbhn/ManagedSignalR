using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Hubs;
public class CustomIdentityResolver : IIdentityProvider
{
    public string GetUserId(HubCallerContext context) => ManagedLib.ManagedSignalR.Constants.Anonymous;
}
