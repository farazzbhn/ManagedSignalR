using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Exceptions;
    public class MissingHandlerException(Type messageType) 
        : Exception($"Handler not registered for incoming message type {messageType}.");
