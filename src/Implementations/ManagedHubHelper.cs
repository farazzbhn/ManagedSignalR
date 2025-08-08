using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;


internal class ManagedHubHelper<THub> :  ManagedHubHelperBase<THub> where THub : AbstractManagedHub
{
   
}
