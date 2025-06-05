using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Exceptions
{
    public class FailedToMapTypeException : Exception
    {
        public FailedToMapTypeException(Type type) : base($"Invalid mapping configuration for {type}.")
        {
        }
    }
}
