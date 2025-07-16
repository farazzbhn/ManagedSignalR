//using ManagedLib.ManagedSignalR.Abstractions;
//using ManagedLib.ManagedSignalR.Configuration;
//using ManagedLib.ManagedSignalR.Types;
//using ManagedLib.ManagedSignalR.Types.Exceptions;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//namespace ManagedLib.ManagedSignalR.Core;

//public class ManagedHubHelper
//{
//    private readonly IServiceProvider _serviceProvider;
//    private readonly IDistributedCacheProvider _cache;
//    private readonly ILogger<ManagedHubHelper> _logger;
//    private readonly ManagedSignalRConfiguration _configuration;
//    private readonly IPublishEndpoint _publisher;

//    public ManagedHubHelper
//    (
//        IServiceProvider serviceProvider,
//        IDistributedCacheProvider cache,
//        ILogger<ManagedHubHelper> logger,
//        ManagedSignalRConfiguration configuration, 
//        IPublishEndpoint publisher
//    )
//    {
//        _serviceProvider = serviceProvider;
//        _cache = cache;
//        _logger = logger;
//        _configuration = configuration;
//        _publisher = publisher;
//    }


//    public Task<ManagedHubSession> GetSession(string userId)
//    {
//        var session = _cache.GetAsync<ManagedHubSession>(userId);
//        return session;
//    } 


//    public async Task<bool> SendToConnectionId<THub>
//    (
//        string userId,
//        string connectionId,
//        object message
//    ) where THub : ManagedHub
//    {
//        // Input validation
//        if (string.IsNullOrWhiteSpace(connectionId))
//            throw new ArgumentNullException(nameof(connectionId));

//        if (message is null)
//            throw new ArgumentNullException(nameof(message));

//        // find the configuration for the hub type and serialize the message 
//        HubEndpointConfiguration config = _configuration.GetConfiguration(typeof(THub));

//        (string Topic, string Payload) serialized = config.Serialize(message);


//        ManagedHubSession? session = await _cache.GetAsync<ManagedHubSession>(userId);

//        Connection? connection = session?.Connections.FirstOrDefault(c => c.ConnectionId == connectionId);

//        if (connection == null) return false; // Connection not found for the user

//        // connection id belongs to this instance => send directly
//        if (connection.InstanceId == Constants.InstanceId)
//        {
//            bool invoked = await TryInvokeClient<THub>(connectionId, serialized.Topic, serialized.Payload);
//            return invoked;
//        }

//        // connection id belongs to another instance => publish message , await the response



//        // Send to the specified connection with error handling
//        bool result = await TryInvokeClient(context, connectionId, Ser, mapping.Serialized, userId);
//        if (!result)
//            throw new Exception($"Failed to send message to connection {connectionId} for user {userId}");
//    }



//    public async Task<bool> TryInvokeClient<THub>
//    (
//        string connectionId,
//        string topic,
//        string payload
//    ) where THub : ManagedHub
//    {
//        try
//        {
//            var context =
//                _serviceProvider.GetService<IHubContext<THub, IManagedHubClient>>();

//            if (context == null)
//                throw new ServiceNotRegisteredException(
//                    $"{typeof(IHubContext<,>).MakeGenericType(typeof(THub), typeof(IManagedHubClient)).FullName}"
//                );

//            await context.Clients.Client(connectionId).InvokeClient(topic, payload);

//            _logger.LogDebug("Successfully sent message to connection {ConnectionId}", connectionId);

//            return true;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogWarning(ex, "Failed to send message to connection {ConnectionId} : {Error}", connectionId, ex.Message);
//            return false;
//        }
//    }

//}
