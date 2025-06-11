using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Exceptions
{
    public class ServiceNotRegisteredException(string type)
        : Exception($"Failed to acquire service {type} from the DI container.");
}
