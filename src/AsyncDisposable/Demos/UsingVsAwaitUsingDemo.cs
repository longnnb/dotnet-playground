namespace AsyncDisposable.Demos;

/// <summary>
/// Demonstrates that a synchronous <c>using</c> block binds to <see cref="IDisposable.Dispose"/>
/// and an <c>await using</c> block binds to <see cref="IAsyncDisposable.DisposeAsync"/>, even
/// though <see cref="AsyncResource"/> implements both -- the keyword at the call site decides
/// which member runs, not which members the type happens to have.
/// </summary>
public static class UsingVsAwaitUsingDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        using (var resource = new AsyncResource("1"))
        {
            _ = resource;
            Console.WriteLine("Using block 1");
        }

        return RunAwaitUsingAsync();
    }

    private static async Task RunAwaitUsingAsync()
    {
        await using (var resource = new AsyncResource("2"))
        {
            _ = resource;
            Console.WriteLine("Await using block 2");
        }
    }
}

/* Expected output
Using block 1
Dispose called from resource '1'
Await using block 2
DisposeAsync called from resource '2'
*/
