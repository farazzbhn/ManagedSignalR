using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;

internal class DefaultManagedHubHelper : IManagedHubHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<DefaultManagedHubHelper> _logger;
    private readonly GlobalConfiguration _globalConfiguration;

    public DefaultManagedHubHelper
    (
        IServiceProvider serviceProvider,
        ICacheProvider cacheProvider,
        ILogger<DefaultManagedHubHelper> logger,
        GlobalConfiguration globalConfiguration
    )
    {
        _serviceProvider = serviceProvider;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _globalConfiguration = globalConfiguration;
    }

    public async Task<int> SendToUser<THub>
    (
        object message,
        string userId
    ) where THub : ManagedHub
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (message is null)
            throw new ArgumentNullException(nameof(message));


        IHubContext<THub, IManagedHubClient>? context =
            _serviceProvider.GetService<IHubContext<THub, IManagedHubClient>>();

        if (context == null)
            throw new ServiceNotRegisteredException(
                $"{typeof(IHubContext<,>).MakeGenericType(typeof(THub), typeof(IManagedHubClient)).FullName}"
            );



        ManagedHubSession? hubConnection = await _cacheProvider.GetAsync<ManagedHubSession>(userId);
        
        // User has no session  => return 0
        if (hubConnection == null || !hubConnection.ConnectionIds.Any())
            return 0;


        (string Topic, string Serialized) mapping = Map<THub>(message);


        // Send to connections with individual error handling
        var sendTasks = new List<Task<bool>>();


        foreach (string connectionId in hubConnection.ConnectionIds)
        {
            sendTasks.Add(
                SendFaraz(context, connectionId, mapping.Topic, mapping.Serialized, userId));
        }

        // Wait for all sends to complete
        bool[] sendResults = await Task.WhenAll(sendTasks);

        // Analyze the results & return
        int attempted = sendResults.Length;
        int sent = sendResults.Count(b => b);

        return sent;
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




    public Task<bool> SendToConnection<THub>(object message, string connectionId) where THub : ManagedHub
    {

    }






    public async Task<bool> SendFaraz<THub>
    (
        IHubContext<THub, IManagedHubClient> context,
        string connectionId,
        string topic,
        string payload,
        string userId
    ) where THub : ManagedHub
    {
        try
        {
            await context.Clients.Client(connectionId).InvokeClient(topic, payload);

            _logger.LogDebug("Successfully sent message to connection {ConnectionId} for user {UserId}", connectionId,
                userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send message to connection {ConnectionId} for user {UserId}: {Error}",
                connectionId, userId, ex.Message);

            return false;
        }
    }
}
