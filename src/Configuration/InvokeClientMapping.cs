namespace ManagedLib.ManagedSignalR.Configuration;
public abstract class InvokeClientMapping
{
    internal string Topic { get; set; }
    internal abstract string Serialize(dynamic obj);
}


public sealed class InvokeClientMapping<TModel> : InvokeClientMapping
{
    internal Func<TModel, string> Serializer { get; private set; } = message => System.Text.Json.JsonSerializer.Serialize(message);
    internal override string Serialize(dynamic obj) => Serializer(obj);

    /// <summary>
    /// <b>Required</b> —
    /// Sets the topic name used when invoking <see cref="IManagedHubClient.InvokeClient"/> 
    /// during the publishing process for this configuration.
    /// </summary>
    public InvokeClientMapping<TModel> RouteToTopic(string topic)
    {
        Topic = topic;
        return this;
    }


    /// <summary>
    /// <b>Optional</b> —
    /// Overrides the default <see cref="System.Text.Json.JsonSerializer"/> 
    /// </summary>
    public InvokeClientMapping<TModel> UseSerializer(Func<TModel, string> serializer)
    {
        Serializer = serializer;
        return this;
    }

    public void ThrowIfInvalid()
    {
        if (Topic == null)
            throw new InvalidOperationException($"Topic not set for type {typeof(TModel).Name}");
    }
}