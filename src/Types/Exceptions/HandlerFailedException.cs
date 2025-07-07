using System;

namespace ManagedLib.ManagedSignalR.Types.Exceptions
{
    public class HandlerFailedException : Exception
    {
        public HandlerFailedException(Type handlerType, Exception innerException)
            : base($"Handler {handlerType} encountered an exception.", innerException)
        {
        }
    }
}