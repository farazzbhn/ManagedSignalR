using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Implementations
{
    internal class DefaultIdentityProvider : IIdentityProvider
    {
        public string GetUserId(HubCallerContext context) => context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Constants.Anonymous;
    }
}
