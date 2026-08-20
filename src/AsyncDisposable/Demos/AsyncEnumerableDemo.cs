namespace AsyncDisposable.Demos;

/// <summary>
/// Demonstrates <c>await foreach</c> over an <see cref="IAsyncEnumerable{T}"/>, including that
/// the compiler calls <see cref="IAsyncDisposable.DisposeAsync"/> on the enumerator when the
/// loop exits -- whether it runs to completion or is abandoned early with <c>break</c>.
/// </summary>
public static class AsyncEnumerableDemo
{
    /// <summary>Runs the loop to completion.</summary>
    public static async Task RunAsync()
    {
        await foreach (var item in CountAsync(3))
        {
            Console.WriteLine($"Received {item}");
        }
    }

    /// <summary>Breaks out of the loop early; the enumerator's <c>finally</c> still runs.</summary>
    public static async Task RunWithEarlyBreakAsync()
    {
        await foreach (var item in CountAsync(3))
        {
            Console.WriteLine($"Received {item}");

            if (item == 1)
            {
                break;
            }
        }
    }

    private static async IAsyncEnumerable<int> CountAsync(int count)
    {
        try
        {
            for (var i = 0; i < count; i++)
            {
                await Task.Delay(20).ConfigureAwait(false);
                yield return i;
            }
        }
        finally
        {
            Console.WriteLine("Iterator finally ran -- the compiler-generated enumerator was disposed");
        }
    }
}

/* Expected output (RunAsync)
Received 0
Received 1
Received 2
Iterator finally ran -- the compiler-generated enumerator was disposed
*/

/* Expected output (RunWithEarlyBreakAsync)
Received 0
Received 1
Iterator finally ran -- the compiler-generated enumerator was disposed
*/
