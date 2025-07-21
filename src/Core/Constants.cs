using System.Security.Cryptography.X509Certificates;

namespace ManagedLib.ManagedSignalR.Core;
public static class Constants
{
    public const string Unauthenticated = "unauthenticated";
    public const int ManagedHubSessionCacheTtl = 10000;
    public const int ManagedHubSessionCacheReInstateInterval = 5000;
}
