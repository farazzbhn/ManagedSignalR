using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Abstractions;
public interface IHubManager
{
    public Task<bool> TryInvokeClient<THub, TMessage>(string userId, TMessage message) where THub : ManagedHub;
}
