using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Core;

public interface IManagedHubContext<THub> where THub : ManagedHub
{
    HubClientsProxy ManagedClients { get; }
}
