using System.Text.Json.Serialization;

namespace ManagedSignalRExample.Models;
public record Alert
{
    public LogLevel Importance { get; set; }
    public string Content { get; set; }
    public string ActionLabel { get; set; }
    public string ActionUrl { get; set; }
}

