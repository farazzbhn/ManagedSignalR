using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedSignalRExample.Hubs;
public class CustomIdentityResolver : IUserIdResolver
{
    public Task<string> GetUserId(HubCallerContext context) => Task.FromResult("anonymous");
}
