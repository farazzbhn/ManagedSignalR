using System.Security.Cryptography.X509Certificates;

namespace ManagedLib.ManagedSignalR.Core;
public static class Constants
{
    public const string Unauthenticated = "unauthenticated";
    public const string CacheKeyPrefix = "msr:";
    public const int SessionTTL = 10000;
    public const int LockTTL = 3000;
}
