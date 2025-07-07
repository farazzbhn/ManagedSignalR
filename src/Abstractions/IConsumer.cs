using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedLib.ManagedSignalR.Types;

namespace ManagedLib.ManagedSignalR.Abstractions;

public abstract class Consumer
{

    public Task<bool> Consume(Envelope envelope)
    {
        if (envelope 
    }
}