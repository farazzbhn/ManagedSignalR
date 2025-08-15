using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// Default implementation of the hub command dispatcher
/// </summary>
public class HubCommandDispatcher : IHubCommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public HubCommandDispatcher
    (
        IServiceProvider serviceProvider
    )
    {
        _serviceProvider = serviceProvider;
    }





    /// <summary>
    /// Dispatches a command to its appropriate handler based on the topic
    /// </summary>
    /// <param name="hubType">The type of the hub that received the message</param>
    /// <param name="topic">The message topic used for routing</param>
    /// <param name="message">The serialized message payload</param>
    /// <param name="context">The SignalR hub context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ServiceNotRegisteredException">
    /// Thrown when no handler is registered for the resolved handler type.
    /// </exception>
    /// <exception cref="HandlerFailedException">
    /// Thrown when the handler's invocation throws an exception.
    /// </exception>
    public async Task FireAndForget(Type hubType, string topic, string message, HubCallerContext context)
    {
        try
        {
            // Retrieve the endpoint configuration configured for this hub
            EndpointOptions configuration = ManagedSignalROptions.Instance.GetEndpointOptions(hubType);

            // retrieve the route object associated with the topic
            if (!configuration.InvokeServerConfigurations.TryGetValue(topic, out InvokeServerConfiguration? route))
                throw new MissingConfigurationException($"No configuration found for topic {topic}. Please ensure it is registered with ConfigureInvokeServer<TModel>() method.");


            dynamic command = route.Deserialize(message);


            // Retrieve the handler type for the topic as registered configuration. 
            // i.e, IHubCommandHandler<Command> where Command is the type of the command being handled.
            Type handlerType = route.HandlerType ?? throw new MisconfiguredException($"Handler type not specified for topic {topic}. Please call UseHandler() to register the respective handler");


            // Get the handler instance from the service provider
            object? handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new ServiceNotRegisteredException(handlerType.ToString());
            }

            MethodInfo? handleMethod = handlerType.GetMethod("Handle");

            if (handleMethod == null)
            {
                throw new MissingMethodException($"Handle method not found on handler type {handlerType}");
            }

            try
            {
                await (Task)handleMethod!.Invoke(handler, [command, context])!;

            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                throw new HandlerFailedException(handlerType, tie.InnerException);
            }
            catch (Exception ex)
            {
                throw new HandlerFailedException(handlerType, ex);
            }
        }
        catch (Exception ex) when (ex is not HandlerFailedException && ex is not ServiceNotRegisteredException)
        {
            throw;
        }
    }
}