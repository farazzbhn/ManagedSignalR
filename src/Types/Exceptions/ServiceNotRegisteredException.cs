using System;

namespace ManagedLib.ManagedSignalR.Types.Exceptions
{
    public class ServiceNotRegisteredException : Exception
    {
        public ServiceNotRegisteredException(string type)
            : base($"Failed to acquire service {type} from the DI container.")
        {
        }
    }
}