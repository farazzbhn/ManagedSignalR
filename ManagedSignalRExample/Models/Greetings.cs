using System.Text.Json.Serialization;

namespace ManagedSignalRExample.Models;
public record Greetings
{
    public string Content { get; set; }
}
