using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Types.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Core;


/// <summary>
/// Dispatches SignalR hub commands by locating and invoking the appropriate
/// <see cref="IHubCommandHandler{TCommand}"/> implementations registered in the service container.
/// </summary>
public sealed class HubCommandDispatcher 
{
    private readonly IServiceProvider _serviceProvider;

    public HubCommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Finds and invokes the first registered handler from the dependency injection container 
    /// that can process the specified command, returning the handler's response.
    /// </summary>
    /// <param name="command">The post command instance to handle.</param>
    public async Task Handle(dynamic command, HubCallerContext context, string userId)
    {
        // Determine the type (e.g., IPostHandler<Location>
        Type handlerType = typeof(IHubCommandHandler<>).MakeGenericType(command.GetType());
        
        // inject the list of registered handlers e.g., ICommandHandler<Request, Response>
        var handlers = (IEnumerable<object>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType));

        if (handlers == null) throw new ServiceNotRegisteredException(handlerType.ToString());

        foreach (var handler in handlers)
        {
            var handleAsyncMethod = handlerType.GetMethod("Handle");

            try
            {
                await (Task) handleAsyncMethod.Invoke(handler, new object[] { command, context, userId});
            }
            catch (Exception exception)
            {
                throw new HandlerFailedException(handlerType, exception);
            }
        }
    }
}
