using System;

namespace ManagedLib.ManagedSignalR.Types.Exceptions
{
    public class HandlerFailedException(Type handlerType, Exception innerException)
        : Exception($"Handler {handlerType} encountered an exception.", innerException);
}