using Microsoft.AspNetCore.SignalR;

namespace ManagedLib.ManagedSignalR.Abstractions;


/// <summary>
/// Handler for processing specific command types
/// </summary>
/// <typeparam name="TCommand">Command type to handle</typeparam>
public interface IHubCommandHandler<in TCommand>
{
    /// <summary>
    /// Handles the specified command asynchronously in a <b>fire &amp; forget </b>manner
    /// </summary>
    /// <param name="request">Command to process</param>
    /// <param name="context">SignalR connection context</param>
    Task Handle(TCommand request, HubCallerContext context, string userId);

}