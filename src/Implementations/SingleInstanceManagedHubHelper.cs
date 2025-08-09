using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;


internal class SingleInstanceManagedHubHelper<THub> :  ManagedHubHelperBase<THub> where THub : AbstractManagedHub
{
    public SingleInstanceManagedHubHelper(ManagedSignalRConfiguration configuration, IHubContext<THub, IManagedHubClient> context, ILogger<ManagedHubHelperBase<THub>> logger, IConnectionManager<THub> connectionManager) : base(configuration, context, logger, connectionManager)
    {
    }

    public override async Task SendToUserAsync(object message, string? userIdentifier, int? maxConcurrency = null)
    {
        string[] connectionIds = await ListConnectionIdsAsync(userIdentifier).ConfigureAwait(false);

        (string Topic, string Payload) serialized = Serialize(message);

        if (!maxConcurrency.HasValue)
        {
            // No concurrency limit, run all tasks in parallel
            var tasks = connectionIds.Select(connectionId =>
                TryInvokeClientAsync(connectionId, serialized.Topic, serialized.Payload));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        else
        {
            // Limit concurrency using SemaphoreSlim
            using var semaphore = new SemaphoreSlim(maxConcurrency.Value);

            var tasks = connectionIds.Select(async connectionId =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                await TryInvokeClientAsync(connectionId, serialized.Topic, serialized.Payload).ConfigureAwait(false);
                semaphore.Release();
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    public override async Task SendToConnectionIdAsync(object message, string connectionId)
    {
        (string Topic, string Payload) serialized = Serialize(message);

        bool invoked = await TryInvokeClientAsync(connectionId, serialized.Topic, serialized.Payload);
    }
}
