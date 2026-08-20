using Microsoft.Extensions.DependencyInjection;

namespace AsyncDisposable.Demos;

/// <summary>
/// Demonstrates <see cref="ServiceProviderServiceExtensions.CreateAsyncScope(IServiceProvider)"/> -- the DI
/// container's counterpart to <c>await using</c> -- and the pitfall it exists to prevent:
/// disposing a <em>synchronous</em> scope that holds an <see cref="IAsyncDisposable"/>-only
/// service throws, because there is no synchronous <c>Dispose</c> to call.
/// </summary>
public static class AsyncScopeDemo
{
    /// <summary>Runs the correct async-scope path, then the synchronous pitfall.</summary>
    public static async Task RunAsync()
    {
        var services = new ServiceCollection();
        services.AddScoped<AsyncOnlyResource>();
        await using var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
        {
            scope.ServiceProvider.GetRequiredService<AsyncOnlyResource>().Use();
        }

        Console.WriteLine("Async scope disposed cleanly");

        try
        {
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AsyncOnlyResource>().Use();
        }
        catch (InvalidOperationException ex)
        {
            // Thrown when the synchronous `using` block ends and the scope tries to Dispose()
            // a service that only implements IAsyncDisposable.
            Console.WriteLine($"Synchronous scope threw as expected: {ex.Message}");
        }
    }
}

// Deliberately not `file`-scoped: this type's name leaks into the InvalidOperationException
// message below, and a `file` type gets a compiler-mangled name that would make that message
// unreadable.
internal sealed class AsyncOnlyResource : IAsyncDisposable
{
    public void Use() => Console.WriteLine("AsyncOnlyResource in use");

    public ValueTask DisposeAsync()
    {
        Console.WriteLine("AsyncOnlyResource.DisposeAsync called");
        return ValueTask.CompletedTask;
    }
}

/* Expected output (the exact InvalidOperationException message text can vary by runtime version)
AsyncOnlyResource in use
AsyncOnlyResource.DisposeAsync called
Async scope disposed cleanly
AsyncOnlyResource in use
Synchronous scope threw as expected: 'AsyncDisposable.Demos.AsyncOnlyResource' type only implements IAsyncDisposable. Use DisposeAsync to dispose the container.
*/
