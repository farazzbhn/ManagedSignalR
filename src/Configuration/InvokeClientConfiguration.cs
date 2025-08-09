using ManagedLib.ManagedSignalR.Core;
using ManagedLib.ManagedSignalR.Types.Exceptions;

namespace ManagedLib.ManagedSignalR.Configuration;
public abstract class InvokeClientConfiguration
{
    internal string? Topic { get; set; } = null;
    internal abstract string Serialize(dynamic obj);
}


public sealed class InvokeClientConfiguration<TModel> : InvokeClientConfiguration
{
    internal Func<TModel, string> Serializer { get; private set; } = message => System.Text.Json.JsonSerializer.Serialize(message);
    internal override string Serialize(dynamic obj) => Serializer(obj);

    /// <summary>
    /// <b>Required</b> —
    /// Sets the topic name used when invoking <see cref="IManagedHubClient.InvokeClient"/> 
    /// during the publishing process for this configuration.
    /// </summary>
    public InvokeClientConfiguration<TModel> RouteToTopic(string topic)
    {
        Topic = topic;
        return this;
    }

    /// <summary>
    /// <b>Optional</b> —
    /// Overrides the default <see cref="System.Text.Json.JsonSerializer"/> 
    /// </summary>
    public InvokeClientConfiguration<TModel> UseSerializer(Func<TModel, string> serializer)
    {
        Serializer = serializer;
        return this;
    }


    /// <summary>
    /// Ensures that the current mapping configuration is complete and valid.
    /// </summary>
    /// <exception cref="MisconfiguredException"></exception>
    internal void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(Topic))
            throw new MisconfiguredException(
                $"Topic is not configured for outgoing message of type '{typeof(TModel).Name}'.\n" +
                $"Use .BindTopic(\"your-topic\") to route the message type ({typeof(TModel).Name}) to a specific topic.");

    }
}