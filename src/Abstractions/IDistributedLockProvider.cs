using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Abstractions;
public interface IDistributedLockProvider
{
    /// <summary>
    /// Attempts to acquire a lock on the specified key. <br /> 
    /// cannot throw
    /// </summary>
    /// <returns>The token if lock was acquired, otherwise null</returns>
    public Task<string?> AcquireAsync(string userId, TimeSpan? timeout = null);


    /// <summary>
    /// Releases the lock if token matches.<br /> 
    /// cannot throw
    /// </summary>
    /// <returns>True if lock was successfully released</returns>
    public Task<bool> ReleaseAsync(string userId, string token);
}
