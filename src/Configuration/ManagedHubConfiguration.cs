using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Exceptions;

namespace ManagedLib.ManagedSignalR.Configuration;
public class ManagedHubConfiguration
{
    private List<EventMapping> Mappings { get; } = new();

    public EventMapping AddHub<T>() where T : ManagedHub<T>
    {
        // Find the EventMapping for the specified hub type, if it exists
        var mapping = Mappings.FirstOrDefault(m => m.Hub == typeof(T));

        // If the mapping doesn't exist, create a new EventMapping
        if (mapping == null)
        {
            mapping = new EventMapping(typeof(T));
            Mappings.Add(mapping);
        }
        return mapping;
    }


    public EventMapping GetMapping(Type type)
    {
        EventMapping? result = Mappings.SingleOrDefault(x => x.Hub == type);
        if (result == null)
        {
            throw new FailedToMapTypeException(type);
        }
        return result;
    }

}
