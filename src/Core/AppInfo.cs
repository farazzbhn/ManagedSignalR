namespace ManagedLib.ManagedSignalR.Core;
public static class AppInfo
{
    /// <summary>
    /// Unique instance id associated with the currently running instance
    /// </summary>
    public static string InstanceId = Guid.NewGuid().ToString("N");
}
