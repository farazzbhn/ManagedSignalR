using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedLib.ManagedSignalR.Types;

namespace ManagedLib.ManagedSignalR.Abstractions;


public interface IEnvelopePublishEndpoint
{
    public Task Publish(Envelope envelope);
}
