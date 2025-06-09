using System.Text.Json.Serialization;

namespace ManagedSignalRExample.Models;
public record NewMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
