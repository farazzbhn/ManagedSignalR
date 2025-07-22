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
        // Determine the type (e.g., IHubCommandHandler<Location>
        Type handlerType = typeof(IHubCommandHandler<>).MakeGenericType(command.GetType());
        
        // inject the list of registered handlers e.g., ICommandHandler<Request, Response>
        object? handler = _serviceProvider.GetService(handlerType);

        if (handler == null) throw new ServiceNotRegisteredException(handlerType.ToString());

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
