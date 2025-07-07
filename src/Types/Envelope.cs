using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Types;
public record Envelope
{
    public string ConnectionId { get; set; }
    public string Topic { get; set; }
    public string Payload { get; set; }
}
