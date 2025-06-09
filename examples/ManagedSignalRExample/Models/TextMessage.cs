using System.Text.Json.Serialization;

namespace ManagedSignalRExample.Models;
public record TextMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
