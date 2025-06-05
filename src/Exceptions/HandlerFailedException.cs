using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Exceptions
{
    public class HandlerFailedException : Exception
    {
        public HandlerFailedException(Type handlerType, Exception innerException) : base($"Handler {handlerType} encountered an exception.", innerException)
        {
        }
    }
}