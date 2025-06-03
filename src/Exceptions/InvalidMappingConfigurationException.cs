using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Exceptions
{
    public class InvalidMappingConfigurationException : Exception
    {
        public InvalidMappingConfigurationException(Type type) : base($"Invalid mapping configuration for {type}.")
        {
        }
    }
}
