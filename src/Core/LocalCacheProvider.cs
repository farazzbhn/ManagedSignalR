namespace ManagedLib.ManagedSignalR.Core;

using System.Collections.Concurrent;

public class LocalCacheProvider<T>
{
    private readonly ConcurrentBag<T> _list = new();

    public void Set(T item) => _list.Add(item);

    public bool Remove(T item) =>_list.TryTake(out item);

    public List<T> List() => _list.ToList();
}

