using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Types.Exceptions;

namespace ManagedLib.ManagedSignalR.Configuration;

public abstract class InvokeServerConfiguration
{
    internal string? Topic { get; set; } = null;
    internal Type? HandlerType { get; set; } = null;
    internal abstract dynamic Deserialize(string payload);
}

public sealed class InvokeServerConfiguration<TModel> : InvokeServerConfiguration
{
    internal Func<string, TModel> Deserializer { get; private set; } = message => System.Text.Json.JsonSerializer.Deserialize<TModel>(message)!;

    internal override dynamic Deserialize(string payload) => Deserializer(payload)!;

    /// <summary>
    /// <b>Required</b> —
    /// Sets the topic for incoming messages
    /// </summary>
    public InvokeServerConfiguration<TModel> OnTopic(string topic)
    {
        Topic = topic;
        return this;
    }

    /// <summary>
    /// <b>Optional</b> —
    /// Overrides the default <see cref="System.Text.Json.JsonSerializer"/> 
    /// </summary>
    public InvokeServerConfiguration<TModel> UseDeserializer(Func<string, TModel> deserializer)
    {
        this.Deserializer = deserializer;
        return this;
    }

    /// <summary>
    /// <b>Required</b> | Sets the handler type for processing messages 
    /// </summary>
    public InvokeServerConfiguration<TModel> UseHandler<THandler>() where THandler : IHubCommandHandler<TModel>
    {
        HandlerType = typeof(THandler);
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
                $"Topic is not configured for message type '{typeof(TModel).Name}'.\n" +
                $"Use .OnTopic(\"your-topic\") to bind the message type ({typeof(TModel).Name}) to a specific topic.");

        if (HandlerType == null)
            throw new MisconfiguredException(
                $"Handler for '{typeof(TModel).Name}' is not registered.\n" +
                $"You must specify a concrete handler using .UseHandler<YourHandler>() where YourHandler : IHubCommandHandler<{typeof(TModel).Name}>."
            );
    }

}