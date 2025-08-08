using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Dispatches commands to their appropriate handlers
/// </summary>
public interface IHubCommandDispatcher
{
    /// <summary>
    /// Deserializes the payload into a command using the , and delegates it to appropriate handler based on the topic
    /// </summary>
    /// <param name="hubType">The type of the hub that received the message</param>
    /// <param name="topic">The message topic used for routing</param>
    /// <param name="message">The serialized message payload</param>
    /// <param name="context">The SignalR hub context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task FireAndForget(Type hubType, string topic, string message, HubCallerContext context);
} 