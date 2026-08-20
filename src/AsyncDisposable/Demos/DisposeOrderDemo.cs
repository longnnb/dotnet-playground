namespace AsyncDisposable.Demos;

/// <summary>
/// Demonstrates that nested <c>await using</c> declarations dispose in LIFO order -- the
/// reverse of declaration order -- exactly like nested synchronous <c>using</c> blocks.
/// </summary>
public static class DisposeOrderDemo
{
    /// <summary>Runs the demo.</summary>
    public static async Task RunAsync()
    {
        await using var outer = new AsyncResource("outer");
        await using var middle = new AsyncResource("middle");
        await using var inner = new AsyncResource("inner");

        Console.WriteLine("Body running with all three resources open");
    }
}

/* Expected output
Body running with all three resources open
DisposeAsync called from resource 'inner'
DisposeAsync called from resource 'middle'
DisposeAsync called from resource 'outer'
*/
