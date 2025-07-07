using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Configuration;

public abstract class InvokeServerMapping
{
    internal string Topic { get; set; }
    internal Type? HandlerType { get; set; }
    internal abstract dynamic Deserialize(string payload);
}

public sealed class InvokeServerMapping<TModel> : InvokeServerMapping
{
    internal Func<string, TModel> Deserializer { get; private set; } = message => System.Text.Json.JsonSerializer.Deserialize<TModel>(message)!;
    internal override dynamic Deserialize(string payload) => Deserializer(payload)!;

    /// <summary>
    /// <b>Required</b> —
    /// Sets the topic for incoming messages
    /// </summary>
    public InvokeServerMapping<TModel> FromTopic(string topic)
    {
        Topic = topic;
        return this;
    }

    /// <summary>
    /// <b>Optional</b> —
    /// Overrides the default <see cref="System.Text.Json.JsonSerializer"/> 
    /// </summary>
    public InvokeServerMapping<TModel> UseDeserializer(Func<string, TModel> deserializer)
    {
        this.Deserializer = deserializer;
        return this;
    }

    /// <summary>
    /// <b>Required</b> | Sets the handler type for processing messages 
    /// </summary>
    public InvokeServerMapping<TModel> UseHandler<THandler>() where THandler : IHubCommandHandler<TModel>
    {
        HandlerType = typeof(THandler);
        return this;
    }


    public void ThrowIfInvalid()
    {
        if (Topic == null)
            throw new InvalidOperationException($"Topic not set for type {typeof(TModel).Name}");

        if (HandlerType == null)
            throw new InvalidOperationException($"IHubCommandHandler not specified for type {typeof(TModel).Name}");
    }

}