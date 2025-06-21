using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;

internal class HubMediator : IHubMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<HubMediator> _logger;
    private readonly GlobalConfiguration _globalConfiguration;

    public HubMediator
    (
        IServiceProvider serviceProvider,
        ICacheProvider cacheProvider,
        ILogger<HubMediator> logger,
        GlobalConfiguration globalConfiguration
    )
    {
        _serviceProvider = serviceProvider;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _globalConfiguration = globalConfiguration;
    }



    public Task<bool> SendToConnectionId<THub>(object message, string connectionId) where THub : ManagedHub
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentNullException(nameof(connectionId));

        if (message is null)
            throw new ArgumentNullException(nameof(message));

        (string Topic, string Serialized) mapping = Map<THub>(message);

        return TryInvokeClient<THub>(connectionId, mapping.Topic, mapping.Serialized);
    }




    private (string Topic, string Serialized) Map<THub>(object message)
    {
        ManagedHubConfiguration? configuration = _globalConfiguration.FindConfiguration(typeof(THub));

        if (configuration?.Outbound == null ||
            !configuration.Outbound.TryGetValue(message.GetType(), out var config))
        {
            throw new MissingConfigurationException(
                $"Configuration not found for {message.GetType()} in {typeof(THub).Name}");
        }

        string topic = config.Topic;
        string serialized = config.Serializer(message);

        return new ValueTuple<string, string>(topic, serialized);
    }

    private async Task<bool> TryInvokeClient<THub>(string connectionId, string topic, string payload) where THub : ManagedHub
    {

        IHubContext<THub, IManagedHubClient>? context =
            _serviceProvider.GetService<IHubContext<THub, IManagedHubClient>>();

        if (context == null)
            throw new ServiceNotRegisteredException(
                $"{typeof(IHubContext<,>).MakeGenericType(typeof(THub), typeof(IManagedHubClient)).FullName}"
            );

        try
        {
            await context.Clients.Client(connectionId).InvokeClient(topic, payload);
            _logger.LogDebug("Successfully invoked \"OnInvokeClient\" on {ConnectionId}", connectionId);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to invoke \"OnInvokeClient\" on {ConnectionId}\nException:\t{Exception}", connectionId, exception.Message);
            return false;
        }
    }
}


