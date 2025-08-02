using ManagedLib.ManagedSignalR.Types;

namespace ManagedLib.ManagedSignalR.Abstractions;


public interface IMessagePublisher
{
    public Task Publish(Envelope envelope);
}
