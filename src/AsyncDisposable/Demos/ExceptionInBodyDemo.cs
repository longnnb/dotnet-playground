namespace AsyncDisposable.Demos;

/// <summary>
/// Demonstrates that <c>await using var</c> disposes the resource -- via
/// <see cref="IAsyncDisposable.DisposeAsync"/> -- before the enclosing <c>finally</c> block
/// runs, whether the method returns normally or throws. The two outcomes are two separate
/// dispatcher entries rather than one commented out, so both are actually observable.
/// </summary>
public static class ExceptionInBodyDemo
{
    /// <summary>Runs the success path: the method returns normally.</summary>
    public static async Task RunAsync()
    {
        Console.WriteLine(await RunMethodAsync(throwException: false));
    }

    /// <summary>Runs the failure path: the method throws before it can return.</summary>
    public static async Task RunThrowingVariantAsync()
    {
        Console.WriteLine(await RunMethodAsync(throwException: true));
    }

    private static async Task<string> RunMethodAsync(bool throwException)
    {
        try
        {
            await using var resource = new AsyncResource("3");
            Console.WriteLine("Await using block in test method 3");

            if (throwException)
            {
                throw new InvalidOperationException("Test error message");
            }

            return "Normal result";
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            return "Exception result";
        }
        finally
        {
            Console.WriteLine("Finally block");
        }
    }
}

/* Expected output (RunAsync)
Await using block in test method 3
DisposeAsync called from resource '3'
Finally block
Normal result
*/

/* Expected output (RunThrowingVariantAsync)
Await using block in test method 3
DisposeAsync called from resource '3'
Test error message
Finally block
Exception result
*/
