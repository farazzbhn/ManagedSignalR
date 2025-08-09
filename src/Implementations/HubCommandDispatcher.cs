using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ManagedLib.ManagedSignalR.Implementations;

/// <summary>
/// Default implementation of the hub command dispatcher
/// </summary>
public class HubCommandDispatcher : IHubCommandDispatcher
{
    private readonly ManagedSignalRConfiguration _config;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HubCommandDispatcher> _logger;

    public HubCommandDispatcher
    (
        ManagedSignalRConfiguration config,
        IServiceProvider serviceProvider,
        ILogger<HubCommandDispatcher> logger
    )
    {
        _config = config;
        _serviceProvider = serviceProvider;
        _logger = logger;
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
            EndpointConfiguration configuration = _config.FetchEndpointConfiguration(hubType);

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
                _logger.LogError("Handler {HandlerType} is not registered in the service provider", handlerType);
                throw new ServiceNotRegisteredException(handlerType.ToString());
            }

            MethodInfo? handleMethod = handlerType.GetMethod("Handle");

            if (handleMethod == null)
            {
                _logger.LogError("Handle method not found on handler type {HandlerType}", handlerType);
                throw new MissingMethodException($"Handle method not found on handler type {handlerType}");
            }

            _logger.LogDebug("Dispatching command of type {CommandType} to handler {HandlerType} for topic {Topic} on hub {HubType}",
                (object)command.GetType().Name, handlerType.Name, topic, hubType.Name);

            try
            {
                await (Task)handleMethod!.Invoke(handler, [command, context])!;

                _logger.LogDebug("Successfully dispatched command of type {CommandType} to handler {HandlerType} for topic {Topic} on hub {HubType}",
                    (object)command.GetType().Name, handlerType.Name, topic, hubType.Name);
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                _logger.LogError(tie.InnerException, "Handler {HandlerType} failed to process command for topic {Topic} on hub {HubType}",
                    handlerType.Name, topic, hubType.Name);
                throw new HandlerFailedException(handlerType, tie.InnerException);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler {HandlerType} failed to process command for topic {Topic} on hub {HubType}",
                    handlerType.Name, topic, hubType.Name);
                throw new HandlerFailedException(handlerType, ex);
            }
        }
        catch (Exception ex) when (ex is not HandlerFailedException && ex is not ServiceNotRegisteredException)
        {
            _logger.LogError(ex, "Unexpected error occurred while dispatching command for topic {Topic} on hub {HubType}", topic, hubType.Name);
            throw;
        }
    }
}