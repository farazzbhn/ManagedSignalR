namespace ManagedLib.ManagedSignalR.Core;

using System.Collections.Concurrent;

public class MemoryCache<T>
{
    private readonly ConcurrentBag<T> _list = new();

    public void Add(T item) => _list.Add(item);

    public bool Remove(T item) =>_list.TryTake(out item);

    public List<T> List() => _list.ToList();
}

