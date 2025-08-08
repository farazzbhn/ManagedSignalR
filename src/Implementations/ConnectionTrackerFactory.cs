using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Implementations;

internal class ConnectionTrackerFactory : IConnectionTrackerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ConnectionTrackerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IConnectionTracker CreateTracker<THub>() where THub : ManagedHub
    {
        return _serviceProvider.GetRequiredService<IConnectionTracker<THub>>();
    }

    public IConnectionTracker CreateTracker(Type hubType)
    {
        var trackerType = typeof(IConnectionTracker<>).MakeGenericType(hubType);
        return (IConnectionTracker)_serviceProvider.GetRequiredService(trackerType);
    }
} 