using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Core;
public class ManagedHubContext<THub> : IManagedHubContext<THub> where THub : ManagedHub
{
    private readonly IHubContext<THub, IManagedHubClient> _hubContext; 

    public ManagedHubContext(IHubContext<THub, IManagedHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public HubClientsProxy ManagedClients => new HubClientsProxy(_hubContext.Clients, typeof(THub));
}
