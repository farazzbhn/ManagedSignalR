using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;
internal class HubManager : IHubManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<HubManager> _logger;
    private readonly GlobalSettings _globalSettings;


    public HubManager
    (
        IServiceProvider serviceProvider, 
        ICacheProvider cacheProvider,
        ILogger<HubManager> logger, 
        GlobalSettings globalSettings
    )
    {
        _serviceProvider = serviceProvider;
        _cacheProvider = cacheProvider;
        _logger = logger;
        _globalSettings = globalSettings;
    }

    public async Task<InvokeResult> TryInvokeClient<THub, TMessage>(
        string userId,
        TMessage message,
        CancellationToken cancellationToken = default)
        where THub : ManagedHub
    {
        var result = new InvokeResult();

        try
        {
            // 1. Input validation
            if (string.IsNullOrWhiteSpace(userId))
            {
                result.AddError("UserId cannot be null or empty");
                return result;
            }

            if (message == null)
            {
                result.AddError("Message cannot be null");
                return result;
            }

            // 2. Get services with proper error handling
            var context = _serviceProvider.GetService<IHubContext<THub, IManagedHubClient>>();
            if (context == null)
            {
                result.AddError($"Hub context for {typeof(THub).Name} not found");
                return result;
            }

            // 3. Get user connections with timeout
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            ManagedHubSession? hubConnection;
            try
            {
                hubConnection = await _cacheProvider.GetAsync<ManagedHubSession>(userId, combinedCts.Token);
            }
            catch (OperationCanceledException)
            {
                result.AddError("Timeout retrieving user connections");
                return result;
            }

            // 4. Validate connections exist and are recent
            if (hubConnection == null || hubConnection.ConnectionIds.Count == 0)
            {
                _logger.LogWarning("User {UserId} has no active connections to {Hub}", userId, typeof(THub).Name);
                result.AddError("No active connections found");
                return result;
            }


            // 5. Get configuration
            ManagedHubConfiguration? configuration = _globalSettings.FindConfiguration(typeof(THub));
            if (configuration?.Client == null ||
                !configuration.Client.TryGetValue(typeof(TMessage), out var bindings))
            {
                throw new MissingConfigurationException($"Configuration not found for {typeof(TMessage).Name} in {typeof(THub).Name}");
            }

            // 6. Serialize with error handling
            string serialized;
            try
            {
                serialized = bindings.Serializer(message);
                if (string.IsNullOrEmpty(serialized))
                {
                    result.AddError("Message serialization resulted in empty string");
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.AddError($"Failed to serialize message: {ex.Message}");
                _logger.LogError(ex, "Failed to serialize message of type {MessageType}", typeof(TMessage).Name);
                return result;
            }

            // 7. Send to connections with individual error handling
            var sendTasks = new List<Task<ConnectionSendResult>>();

            foreach (string connectionId in hubConnection.ConnectionIds)
            {
                sendTasks.Add(SendToConnection(context, connectionId, bindings.Topic, serialized, userId, combinedCts.Token));
            }

            // 9. Wait for all sends to complete
            var sendResults = await Task.WhenAll(sendTasks);

            // 10. Analyze results
            result.TotalAttempted = sendResults.Length;
            result.SuccessfulSends = sendResults.Count(r => r.Success);
            result.FailedSends = sendResults.Count(r => !r.Success);

            // 11. Log failed connections for cleanup
            var failedConnections = sendResults.Where(r => !r.Success).Select(r => r.ConnectionId).ToList();
            if (failedConnections.Any())
            {
                _logger.LogWarning("Failed to send to connections: {FailedConnections} for user {UserId}",
                    string.Join(", ", failedConnections), userId);

                // 12. Clean up stale connections (fire and forget)
                _ = Task.Run(async () => await CleanupStaleConnections(userId, failedConnections), CancellationToken.None);
            }

            result.Success = result.SuccessfulSends > 0;

            _logger.LogInformation("Message sent to {SuccessfulSends}/{TotalAttempted} connections for user {UserId}",
                result.SuccessfulSends, result.TotalAttempted, userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in TryInvokeClient for user {UserId}", userId);
            result.AddError($"Unexpected error: {ex.Message}");
            return result;
        }
    }

    private async Task<ConnectionSendResult> SendToConnection<THub>(
        IHubContext<THub, IManagedHubClient> context,
        string connectionId,
        string topic,
        string serializedMessage,
        string userId,
        CancellationToken cancellationToken)
    where THub : ManagedHub
    {
        try
        {
            // Add timeout for individual connection sends
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await context.Clients.Client(connectionId).InvokeClient(topic, serializedMessage);

            _logger.LogDebug("Successfully sent message to connection {ConnectionId} for user {UserId}",
                connectionId, userId);

            return new ConnectionSendResult { ConnectionId = connectionId, Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send message to connection {ConnectionId} for user {UserId}: {Error}",
                connectionId, userId, ex.Message);

            return new ConnectionSendResult
            {
                ConnectionId = connectionId,
                Success = false,
                Error = ex.Message
            };
        }
    }

    private async Task CleanupStaleConnections(string userId, List<string> staleConnectionIds)
    {
        try
        {
            var hubConnection = await _cacheProvider.GetAsync<ManagedHubSession>(userId);
            if (hubConnection != null)
            {
                // Remove stale connections
                foreach (var staleId in staleConnectionIds)
                {
                    hubConnection.ConnectionIds.Remove(staleId);
                }

                // Update cache
                if (hubConnection.ConnectionIds.Any())
                {
                    await _cacheProvider.SetAsync(userId, hubConnection);
                }
                else
                {
                    await _cacheProvider.RemoveAsync(userId);
                }

                _logger.LogInformation("Cleaned up {Count} stale connections for user {UserId}",
                    staleConnectionIds.Count, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup stale connections for user {UserId}", userId);
        }
    }

    // Supporting classes
    public class InvokeResult
    {
        public bool Success { get; set; }
        public int TotalAttempted { get; set; }
        public int SuccessfulSends { get; set; }
        public int FailedSends { get; set; }
        public List<string> Errors { get; set; } = new();

        public void AddError(string error) => Errors.Add(error);
        public bool HasErrors => Errors.Any();
    }

    public class ConnectionSendResult
    {
        public string ConnectionId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    // Optional: Add retry mechanism
    public async Task<InvokeResult> TryInvokeClientWithRetry<THub, TMessage>(
        string userId,
        TMessage message,
        int maxRetries = 2,
        TimeSpan retryDelay = default,
        CancellationToken cancellationToken = default)
        where THub : ManagedHub
    {
        if (retryDelay == default) retryDelay = TimeSpan.FromSeconds(1);

        InvokeResult? lastResult = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            lastResult = await TryInvokeClient<THub, TMessage>(userId, message, cancellationToken);

            if (lastResult.Success || attempt == maxRetries)
            {
                break;
            }

            _logger.LogInformation("Retry attempt {Attempt}/{MaxRetries} for user {UserId} after {Delay}ms",
                attempt + 1, maxRetries, userId, retryDelay.TotalMilliseconds);

            await Task.Delay(retryDelay, cancellationToken);
        }

        return lastResult!;
    }

    // Optional: Add message queuing for offline users
    public async Task<bool> QueueMessageForOfflineUser<TMessage>(string userId, TMessage message)
    {
        try
        {
            var queueKey = $"offline_messages:{userId}";
            var queuedMessage = new QueuedMessage
            {
                MessageType = typeof(TMessage).Name,
                Payload = JsonSerializer.Serialize(message),
                Timestamp = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // Messages expire after 24 hours
            };

            // Add to queue (implementation depends on your cache provider)
            await _cacheProvider.ListAddAsync(queueKey, queuedMessage);

            _logger.LogInformation("Queued message for offline user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue message for offline user {UserId}", userId);
            return false;
        }
    }

    public class QueuedMessage
    {
        public string MessageType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime ExpiresAt { get; set; }
    }












}
