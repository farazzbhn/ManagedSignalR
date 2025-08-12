using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Core;

public interface IManagedHubContext<THub> where THub : ManagedHub
{
    HubClientsProxy Clients { get; }
    IGroupManager Groups { get; }
}
