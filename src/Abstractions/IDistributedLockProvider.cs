using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Abstractions;
public interface IDistributedLockProvider
{
    /// <summary>
    /// Attempts to acquire a lock on the specified key.
    /// </summary>
    /// <returns>The token if lock was acquired, otherwise null</returns>
    public Task<string?> WaitAsync(string key, TimeSpan? timeout = null);


    /// <summary>
    /// Releases the lock if token matches.
    /// </summary>
    /// <returns>True if lock was successfully released</returns>
    public Task<bool> ReleaseAsync(string key, string token);
}
