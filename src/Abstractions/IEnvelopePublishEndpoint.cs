using ManagedLib.ManagedSignalR.Types;

namespace ManagedLib.ManagedSignalR.Abstractions;


public interface IEnvelopePublishEndpoint
{
    public Task Publish(Envelope envelope);
}
