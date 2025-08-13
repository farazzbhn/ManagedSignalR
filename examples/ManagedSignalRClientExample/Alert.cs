using Microsoft.Extensions.Logging;

namespace ManagedSignalRClientExample;

public record Alert
{
    public string Content { get; set; }
    public string ActionLabel { get; set; }
}

