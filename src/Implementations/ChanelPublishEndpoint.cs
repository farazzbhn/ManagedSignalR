using System;
using System.Threading.Channels;
using ManagedLib.ManagedSignalR.Types;
using ManagedLib.ManagedSignalR.Abstractions;

namespace ManagedLib.ManagedSignalR.Implementations;
internal class ChanelPublishEndpoint : IEnvelopePublishEndpoint
{
    private readonly ChannelWriter<Envelope> _writer;

    public ChanelPublishEndpoint(ChannelWriter<Envelope> writer)
    {
        _writer = writer;
    }

    public async Task Publish(Envelope envelope)
    {
        if (envelope == null)
            throw new ArgumentNullException(nameof(envelope));

        // Waits if the channel is full (bounded) or completes immediately (unbounded)
        await _writer.WriteAsync(envelope);
    }
}
