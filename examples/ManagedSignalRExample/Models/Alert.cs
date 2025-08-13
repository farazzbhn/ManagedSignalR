using System.Text.Json.Serialization;

namespace ManagedSignalRExample.Models;
public record Alert
{
    public string Content { get; set; }
    public string ActionLabel { get; set; }
}

