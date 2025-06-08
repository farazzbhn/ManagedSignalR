using System.Text.Json.Serialization;

namespace ManagedSignalRExample.Models;
public record IncomingMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
