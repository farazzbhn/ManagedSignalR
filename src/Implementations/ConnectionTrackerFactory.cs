using ManagedLib.ManagedSignalR.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedLib.ManagedSignalR.Implementations;
internal class TrackerFactory : IConnectionTrackerFactory
{
    private readonly IServiceProvider _provider;

    internal TrackerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IConnectionTracker GetTracker(Type hubType)
    {
        if (!typeof(AbstractManagedHub).IsAssignableFrom(hubType))
            throw new InvalidOperationException($"{hubType.Name} does not implement {nameof(AbstractManagedHub)}");

        Type trackerType = typeof(IConnectionTracker<>).MakeGenericType(hubType);

        return (IConnectionTracker)_provider.GetRequiredService(trackerType);
    }
}