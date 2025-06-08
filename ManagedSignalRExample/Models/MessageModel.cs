using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ManagedSignalRExample.Models;
public record Message
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("role")]
    public Role Role { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}

public enum Role
{
    User,
    System,
}
