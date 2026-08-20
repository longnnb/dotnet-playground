namespace Challenges;

/// <summary>Finds the maximum value in a 2-D array.</summary>
public static class Exercise
{
    /// <summary>
    /// Returns the largest value in <paramref name="numbers"/>. Throws for an empty array
    /// instead of returning a sentinel -- the original returned <c>-1</c>, which is
    /// indistinguishable from a legitimate maximum when every element is negative.
    /// </summary>
    public static int FindMax(int[,] numbers)
    {
        if (numbers.Length == 0)
        {
            throw new ArgumentException("Array must contain at least one element.", nameof(numbers));
        }

        var max = int.MinValue;

        foreach (var n in numbers)
        {
            if (n > max)
            {
                max = n;
            }
        }

        return max;
    }

    /// <summary>
    /// Runs <see cref="FindMax"/> over a normal array, an all-negative array (whose correct
    /// answer collides with the old sentinel value), and an empty array (which now throws).
    /// </summary>
    public static Task RunAsync()
    {
        int[,] grid = { { 3, -7, 2 }, { -1, 9, 4 } };
        Console.WriteLine($"FindMax(normal grid) = {FindMax(grid)}");

        int[,] allNegative = { { -5, -2 }, { -9, -1 } };
        Console.WriteLine(
            $"FindMax(all-negative grid) = {FindMax(allNegative)} " +
            "(the old code returned -1 for an EMPTY array too -- indistinguishable from this legitimate answer)");

        try
        {
            FindMax(new int[0, 0]);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"FindMax(empty grid) now throws instead of returning a sentinel: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}
