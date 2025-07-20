using System.Security.Cryptography.X509Certificates;

namespace ManagedLib.ManagedSignalR.Core;
public static class Constants
{
    public const string Unauthenticated = "unauthenticated";
    public const int SessionTtl = 10000;
    public const int CacheEntryInterval = 5000;
    public const int LockTTL = 5000;
}
