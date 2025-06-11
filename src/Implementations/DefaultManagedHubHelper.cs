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
    private readonly GlobalSettings _globalSettings;

    public DefaultManagedHubHelper
    (
        IServiceProvider serviceProvider,
        ICacheProvider cacheProvider,
        ILogger<DefaultManagedHubHelper> logger,
        GlobalSettings globalSettings
    )
    {
        _serviceProvider = serviceProvider;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _globalSettings = globalSettings;
    }

    public async Task<int> InvokeClientAsync<THub, TMessage>
    (
        string userId,
        TMessage message
    ) where THub : ManagedHub
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (message == null)
            throw new ArgumentNullException(nameof(userId));


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

        // Get configuration
        ManagedHubConfiguration? configuration = _globalSettings.FindConfiguration(typeof(THub));

        if (configuration?.Client == null ||
            !configuration.Client.TryGetValue(typeof(TMessage), out var bindings))
        {
            throw new MissingConfigurationException(
                $"Configuration not found for {typeof(TMessage).Name} in {typeof(THub).Name}");
        }

        // Serialize => throws on failure
        string serialized = bindings.Serializer(message);


        // Send to connections with individual error handling
        var sendTasks = new List<Task<bool>>();

        foreach (string connectionId in hubConnection.ConnectionIds)
        {
            sendTasks.Add(
                SendToConnection(context, connectionId, bindings.Topic, serialized, userId));
        }

        // Wait for all sends to complete
        bool[] sendResults = await Task.WhenAll(sendTasks);

        // Analyze the results & return
        int attempted = sendResults.Length;
        int sent = sendResults.Count(b => b);

        return sent;
    }


    private async Task<bool> SendToConnection<THub>
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
