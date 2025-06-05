using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Exceptions;

namespace ManagedLib.ManagedSignalR;

public sealed class HandlerBus 
{
    private readonly IServiceProvider _serviceProvider;

    public HandlerBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Finds and invokes the first registered handler from the dependency injection container 
    /// that can process the specified command, returning the handler's response.
    /// </summary>
    /// <param name="command">The post command instance to handle.</param>
    public async Task HandleAsync(dynamic command)
    {
        // Determine the type (e.g., IPostHandler<Location>
        Type handlerType = typeof(IHandler<>).MakeGenericType(command.GetType());
        
        // inject the list of registered handlers e.g., ICommandHandler<Request, Response>
        var handlers = (IEnumerable<object>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType));

        if (handlers == null) throw new HandlerNotFoundException(command.GetType());

        foreach (var handler in handlers)
        {
            var handleAsyncMethod = handlerType.GetMethod("SubmitAsync");

            try
            {
                await (Task) handleAsyncMethod.Invoke(handler, new object[] { command });
            }
            catch (Exception exception)
            {
                throw new HandlerFailedException(handlerType, exception);
            }
        }
    }
}
