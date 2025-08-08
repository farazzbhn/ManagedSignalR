namespace ManagedLib.ManagedSignalR.Abstractions;

/// <summary>
/// Provides a type-safe abstraction for retrieving the appropriate <see cref="IConnectionTracker"/> instance
/// for a given <see cref="AbstractManagedHub"/> at runtime.
/// </summary>
/// <remarks>
/// Used to resolve <c>IConnectionTracker&lt;THub&gt;</c> implementations without exposing open generics
/// to consumers. Typically implemented using a cached factory pattern to avoid repeated reflection-based resolution.
/// </remarks>
internal interface IConnectionTrackerFactory
{
    /// <summary>
    /// Returns the <see cref="IConnectionTracker"/> instance associated with the specified <see cref="AbstractManagedHub"/> type.
    /// </summary>
    /// <param name="hubType">The concrete type of the SignalR hub.</param>
    /// <returns>A runtime-safe tracker instance responsible for managing hub connections.</returns>
    internal IConnectionTracker GetTracker(Type hubType);
}
