using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Abstractions;
internal interface IManagedHubHelper
{
    internal Task SendToUser<THub>(string userId) where THub : ManagedHub;
}
