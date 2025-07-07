namespace ManagedLib.ManagedSignalR.Core
{
    public static class Constants
    {
        public const string Anonymous = "anon";
        public const string Prefix = "msr:";
        public static string InstanceId = Guid.NewGuid().ToString("N");
    }
}
