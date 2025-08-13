using System;

namespace ManagedLib.ManagedSignalR.Types.Exceptions
{
    public class ServiceNotRegisteredException(string type)
        : Exception($"Failed to acquire service {type} from the DI container.");
}