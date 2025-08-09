using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Configuration;
using ManagedLib.ManagedSignalR.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManagedLib.ManagedSignalR.Implementations;


internal class SingleInstanceAbstractManagedHubHelper<THub> :  ManagedHubHelperBase<THub> where THub : ManagedHub
{
    public SingleInstanceAbstractManagedHubHelper(IConnectionManager<THub> connections, ILogger<ManagedHubHelperBase<THub>> logger, IHubContext<THub, IManagedHubClient> context) : base(connections, logger, context)
    {
    }

    public override async Task SendToUserAsync(object message, string? userIdentifier, int? maxConcurrency = null)
    {
        string[] connections = Connections.UserConnections(userIdentifier);

        (string Topic, string Payload) serialized = Serialize(message);

        if (!maxConcurrency.HasValue)
        {
            // No concurrency limit, run all tasks in parallel
            var tasks = connections.Select(connectionId =>
                TryInvokeClientAsync(connectionId, serialized.Topic, serialized.Payload));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        else
        {
            // Limit concurrency using SemaphoreSlim
            using var semaphore = new SemaphoreSlim(maxConcurrency.Value);

            var tasks = connections.Select(async connectionId =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                bool invoked = await TryInvokeClientAsync(connectionId, serialized.Topic, serialized.Payload).ConfigureAwait(false);
                semaphore.Release();
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    public override async Task SendToConnectionAsync(object message, string connectionId)
    {
        (string Topic, string Payload) serialized = Serialize(message);

        bool invoked = await TryInvokeClientAsync(connectionId, serialized.Topic, serialized.Payload);
    }


}
