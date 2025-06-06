namespace ManagedLib.ManagedSignalR.Abstractions;


/// <summary>
/// Defines a handler capable of processing a specific command type <typeparamref name="TCommand"/> 
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface IHandler<in TCommand> 
{
    /// <summary>
    /// Handles the specified command asynchronously in a <b>fire &amp; forget </b>manner
    /// </summary>
    /// <param name="request">The command instance.</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ProcessAsync(TCommand request);
}