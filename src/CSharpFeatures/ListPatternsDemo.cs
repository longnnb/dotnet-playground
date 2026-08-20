namespace CSharpFeatures;

/// <summary>
/// Demonstrates C# 11 list patterns: matching an array by shape, including the <c>..</c> slice
/// pattern that captures "everything else" without allocating a subarray for it.
/// </summary>
public static class ListPatternsDemo
{
    /// <summary>Runs the demo over arrays of varying length.</summary>
    public static Task RunAsync()
    {
        Describe([]);
        Describe([1]);
        Describe([1, 2]);
        Describe([1, 2, 3]);
        Describe([1, 2, 3, 4, 5]);

        return Task.CompletedTask;
    }

    private static void Describe(int[] values)
    {
        var description = values switch
        {
            [] => "empty",
            [var only] => $"single element: {only}",
            [var first, var second] => $"exactly two: {first}, {second}",
            [var first, .., var last] => $"first={first}, last={last}, {values.Length} elements total",
        };

        Console.WriteLine($"[{string.Join(", ", values)}] => {description}");
    }
}

/* Expected output
[] => empty
[1] => single element: 1
[1, 2] => exactly two: 1, 2
[1, 2, 3] => first=1, last=3, 3 elements total
[1, 2, 3, 4, 5] => first=1, last=5, 5 elements total
*/
