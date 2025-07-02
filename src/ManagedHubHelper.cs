using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR;

public class ManagedHubHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDistributedCacheProvider _disributedCacheProvider;
    private readonly ILogger<ManagedHubHelper> _logger;
    private readonly ManagedSignalRConfiguration _configuration;

    public ManagedHubHelper
    (
        IServiceProvider serviceProvider,
        IDistributedCacheProvider distributedCache,
        ILogger<ManagedHubHelper> logger,
        ManagedSignalRConfiguration configuration
    )
    {
        _serviceProvider = serviceProvider;
        _disributedCacheProvider = distributedCache;
        _logger = logger;
        _configuration = configuration;
    }


    public async Task<bool> SendToConnectionId<THub>
    (
        string userId,
        string connectionId,
        object message
    ) where THub : ManagedHub
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentNullException(nameof(connectionId));

        if (message is null)
            throw new ArgumentNullException(nameof(message));

        IHubContext<THub, IManagedHubClient>? context =
            _serviceProvider.GetService<IHubContext<THub, IManagedHubClient>>();

        if (context == null)
            throw new ServiceNotRegisteredException(
                $"{typeof(IHubContext<,>).MakeGenericType(typeof(THub), typeof(IManagedHubClient)).FullName}"
            );



        // find the configuration for the hub type and serialize the message 

        HubEndpointConfiguration config = _configuration.GetConfiguration(typeof(THub));

        (string Topic, string Payload) serialized = config.Serialize(message);




        ManagedHubSession? session = await _disributedCacheProvider.GetAsync<ManagedHubSession>(userId);
        var connection = session?.Connections.FirstOrDefault(c => c.ConnectionId == connectionId);

        if (connection == null) return false; // Connection not found for the user

        // connection id belongs to this instance => send directly
        if (connection.InstanceId == Constants.InstanceId)
        {
            try
            {
                await context.Clients.Client(connectionId).InvokeClient(serialized.Topic, serialized.Payload);

                _logger.LogDebug("Successfully sent message to connection {ConnectionId} for user {UserId}", connectionId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send message to connection {ConnectionId} for user {UserId}: {Error}", connectionId, userId, ex.Message);
                return false;
            }
        }

        // connection id belongs to another instance => publish message , await the response



        // Send to the specified connection with error handling
        bool result = await InvokeClient(context, connectionId, mapping.Topic, mapping.Serialized, userId);
        if (!result)
            throw new Exception($"Failed to send message to connection {connectionId} for user {userId}");
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


        ManagedHubSession? hubConnection = await _disributedCacheProvider.GetAsync<ManagedHubSession>(userId);

        // User has no session  => return 0
        if (hubConnection == null || !hubConnection.Connections.Any()) return 0;


        (string Topic, string Serialized) mapping = Map<THub>(message);


        // Send to connections with individual error handling
        var sendTasks = new List<Task<bool>>();


        foreach (string connectionId in hubConnection.Connections.Select())
        {
            sendTasks.Add(
                InvokeClient(context, connectionId, mapping.Topic, mapping.Serialized, userId));
        }

        // Wait for all sends to complete
        bool[] sendResults = await Task.WhenAll(sendTasks);

        // Analyze the results & return
        int attempted = sendResults.Length;
        int sent = sendResults.Count(b => b);

        return sent;
    }



    public async Task<bool> InvokeClient<THub>
    (
        IHubContext<THub, IManagedHubClient> context,
        string connectionId,
        string topic,
        string payload,
        string userId
    ) where THub : ManagedHub
    {
 
    }

    public async Task<ManagedHubSession> GetSession(string userId)
    {
        ManagedHubSession? session = await _disributedCacheProvider.GetAsync<ManagedHubSession>(userId);
        return session;
    }
}
