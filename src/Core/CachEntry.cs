namespace ManagedLib.ManagedSignalR.Core;

internal record ManagedHubSessionCacheEntry
{
    
    public ManagedHubSessionCacheEntry(string key, string value, ManagedHubSession session)
    {
        Key = key;
        Value = value;
        Session  = session;

    }

    public ManagedHubSession Session { get; set; }

    public string Key { get; private set; }
    public string Value { get; private set; }

}

