using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Abstractions;

public interface IMessage
{

}

public interface IMessagePublisher
{
    public Task PublishAsync(IMessage message);
}
