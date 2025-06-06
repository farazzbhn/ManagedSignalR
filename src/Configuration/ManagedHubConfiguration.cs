using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Configuration;

public class ManagedHubConfiguration
{
    internal readonly List<EventBinding> Bindings;

    private readonly IServiceCollection _services;

    public ManagedHubConfiguration
    (
        IServiceCollection services
    )
    {
        Bindings = new List<EventBinding>();
        _services = services;
    }


    /// <summary>
    /// Configures a hub with its event mappings
    /// </summary>
    /// <typeparam name="THub">The hub type that inherits from ManagedHub</typeparam>
    /// <returns>An EventMapping instance for fluent configuration</returns>
    public EventBinding AddHub<THub>() where THub : ManagedHub<THub>
    {
        // Find or create mapping for the hub
        var binding = Bindings.FirstOrDefault(m => m.HubType == typeof(THub));

        if (binding == null)
        {
            binding = new EventBinding(typeof(THub), _services);
            Bindings.Add(binding);
        }

        return binding;
    }

    internal EventBinding? GetEventBinding(Type hubType)
    {
        var mapping = Bindings.SingleOrDefault(x => x.HubType == hubType);
        return mapping;
    }
}
