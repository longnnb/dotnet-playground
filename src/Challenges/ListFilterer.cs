namespace Challenges;

/// <summary>
/// Demonstrates <see cref="Enumerable.OfType{TResult}"/> for filtering a heterogeneous list
/// down to one element type, contrasted with the manual <c>is</c> + cast loop it replaces.
/// </summary>
public static class ListFilterer
{
    /// <summary>Filters <paramref name="listOfItems"/> down to its <see cref="int"/> elements using LINQ.</summary>
    public static IEnumerable<int> GetIntegersFromList(List<object> listOfItems) => listOfItems.OfType<int>();

    /// <summary>The same filter written as an explicit loop with a type check and cast.</summary>
    public static IEnumerable<int> GetIntegersFromListWithLoop(List<object> listOfItems)
    {
        var result = new List<int>();

        foreach (var item in listOfItems)
        {
            if (item is int number)
            {
                result.Add(number);
            }
        }

        return result;
    }

    /// <summary>Runs both variants over the same mixed list and compares the results.</summary>
    public static Task RunAsync()
    {
        List<object> mixed = [1, "two", 3, 4.5, "five", 6];

        var viaLinq = GetIntegersFromList(mixed).ToArray();
        var viaLoop = GetIntegersFromListWithLoop(mixed).ToArray();

        Console.WriteLine($"OfType<int>(): [{string.Join(", ", viaLinq)}]");
        Console.WriteLine($"Manual loop:   [{string.Join(", ", viaLoop)}]");
        Console.WriteLine($"[{(viaLinq.SequenceEqual(viaLoop) ? "agree" : "DIFFER")}]");

        return Task.CompletedTask;
    }
}
