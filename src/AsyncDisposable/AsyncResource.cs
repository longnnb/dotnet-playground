namespace AsyncDisposable;

/// <summary>
/// A resource that implements <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> side
/// by side: a synchronous <c>using</c> block binds to <see cref="Dispose()"/>, an
/// <c>await using</c> block binds to <see cref="DisposeAsync"/>, and both funnel through the
/// same idempotent core so the resource is safe to dispose more than once, from either path.
/// </summary>
/// <remarks>
/// The shape here -- <see cref="DisposeAsync"/> awaits <see cref="DisposeAsyncCore"/>, then
/// calls the synchronous <see cref="Dispose(bool)"/> with <c>disposing: false</c>, then calls
/// <see cref="GC.SuppressFinalize"/> -- is the pattern Microsoft documents for a type that
/// wants a real async cleanup path rather than just a <c>ValueTask</c>-wrapped synchronous one.
/// A one-off <c>public async ValueTask DisposeAsync() { ... }</c> with no core method is fine
/// for a sealed leaf type, but doesn't show the extension point a derived type hooks into --
/// see <see cref="DerivedAsyncResource"/>.
/// </remarks>
public class AsyncResource(string name) : IDisposable, IAsyncDisposable
{
    private bool _disposed;

    /// <summary>The name used in this instance's console output, so demos can tell resources apart.</summary>
    protected string Name { get; } = name;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// The synchronous disposal path. <paramref name="disposing"/> is <see langword="true"/>
    /// when called from the synchronous <see cref="Dispose()"/>, and <see langword="false"/>
    /// when called from <see cref="DisposeAsync"/> -- in the latter case the async work already
    /// ran in <see cref="DisposeAsyncCore"/>, so this override only needs to release anything
    /// that doesn't need an async path (there isn't any here).
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Console.WriteLine($"Dispose called from resource '{Name}'");
        }

        _disposed = true;
    }

    /// <summary>
    /// Override point for a derived type's async cleanup. The base implementation performs this
    /// type's own async work (a simulated 100ms flush) and is idempotent on its own.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
        {
            return;
        }

        await Task.Delay(100).ConfigureAwait(false);
        Console.WriteLine($"DisposeAsync called from resource '{Name}'");
    }
}
