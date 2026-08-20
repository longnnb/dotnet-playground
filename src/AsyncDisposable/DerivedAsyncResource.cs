namespace AsyncDisposable;

/// <summary>
/// Demonstrates the extension point in <see cref="AsyncResource"/>: overriding
/// <see cref="AsyncResource.DisposeAsyncCore"/> to run derived-type async cleanup first, then
/// chaining into <c>base.DisposeAsyncCore()</c> so the base type's own cleanup still happens.
/// </summary>
public class DerivedAsyncResource(string name) : AsyncResource(name)
{
    /// <inheritdoc />
    protected override async ValueTask DisposeAsyncCore()
    {
        await Task.Delay(50).ConfigureAwait(false);
        Console.WriteLine($"DerivedAsyncResource '{Name}' released its own resource first");
        await base.DisposeAsyncCore().ConfigureAwait(false);
    }
}
