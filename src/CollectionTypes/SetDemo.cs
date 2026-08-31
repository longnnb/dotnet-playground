namespace CollectionTypes;

/// <summary>
/// Demonstrates <see cref="HashSet{T}"/> and <see cref="SortedSet{T}"/>: the in-place set
/// algebra (<c>UnionWith</c> / <c>IntersectWith</c> / <c>ExceptWith</c> /
/// <c>SymmetricExceptWith</c>) next to its non-mutating LINQ equivalents, the subset/superset
/// predicates and their empty-set edge cases, and what an ordered set buys you
/// (<c>GetViewBetween</c>, <c>Min</c>/<c>Max</c>) at the cost of O(log n) instead of O(1).
/// </summary>
public static class SetDemo
{
    /// <summary>
    /// The <c>...With</c> methods mutate the set they are called on; the LINQ operators
    /// (<c>Union</c>, <c>Intersect</c>, <c>Except</c>) return a new sequence and leave both
    /// inputs alone. For the same inputs they compute the same members.
    /// </summary>
    /// <remarks>
    /// There is no LINQ operator for symmetric difference -- it is spelled as the union minus
    /// the intersection.
    /// </remarks>
    public static Task RunOperationsAsync()
    {
        int[] a = [1, 2, 3, 4, 5];
        int[] b = [4, 5, 6, 7];
        Console.WriteLine($"a = {{{string.Join(", ", a)}}}, b = {{{string.Join(", ", b)}}}");

        Compare("union", MutateWith(a, s => s.UnionWith(b)), [.. a.Union(b)]);
        Compare("intersect", MutateWith(a, s => s.IntersectWith(b)), [.. a.Intersect(b)]);
        Compare("except", MutateWith(a, s => s.ExceptWith(b)), [.. a.Except(b)]);
        Compare("symmetric except", MutateWith(a, s => s.SymmetricExceptWith(b)), [.. a.Union(b).Except(a.Intersect(b))]);

        return Task.CompletedTask;

        static SortedSet<int> MutateWith(int[] seed, Action<HashSet<int>> op)
        {
            var set = new HashSet<int>(seed);
            op(set);
            return [.. set];
        }

        static void Compare(string label, SortedSet<int> mutating, int[] linq)
        {
            var linqSorted = linq.OrderBy(x => x);
            var agree = mutating.SequenceEqual(linqSorted);
            Console.WriteLine($"  {label,-16}: mutating -> {{{string.Join(", ", mutating)}}}, LINQ -> {{{string.Join(", ", linqSorted)}}} [{(agree ? "agree" : "DIFFER")}]");
        }
    }

    /// <summary>
    /// <c>IsSubsetOf</c> / <c>IsSupersetOf</c> / <c>Overlaps</c> / <c>SetEquals</c>, including
    /// the conventions at the empty set: the empty set is a subset of everything (and a proper
    /// subset of any non-empty set), and overlaps nothing.
    /// </summary>
    public static Task RunRelationsAsync()
    {
        var universe = new HashSet<int> { 1, 2, 3, 4, 5 };
        var evens = new HashSet<int> { 2, 4 };
        var withSix = new HashSet<int> { 1, 2, 3, 4, 5, 6 };
        var empty = new HashSet<int>();

        Console.WriteLine($"evens.IsSubsetOf(universe)          = {evens.IsSubsetOf(universe)}");
        Console.WriteLine($"evens.IsProperSubsetOf(universe)    = {evens.IsProperSubsetOf(universe)}");
        Console.WriteLine($"universe.IsSubsetOf(universe)       = {universe.IsSubsetOf(universe)}");
        Console.WriteLine($"universe.IsProperSubsetOf(universe) = {universe.IsProperSubsetOf(universe)}");
        Console.WriteLine($"withSix.IsSupersetOf(universe)      = {withSix.IsSupersetOf(universe)}");
        Console.WriteLine($"evens.Overlaps(universe)            = {evens.Overlaps(universe)}");
        Console.WriteLine($"empty.IsSubsetOf(universe)          = {empty.IsSubsetOf(universe)}");
        Console.WriteLine($"empty.IsProperSubsetOf(universe)    = {empty.IsProperSubsetOf(universe)}");
        Console.WriteLine($"empty.Overlaps(universe)            = {empty.Overlaps(universe)}");
        Console.WriteLine($"universe.SetEquals([5, 4, 3, 2, 1]) = {universe.SetEquals([5, 4, 3, 2, 1])}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="SortedSet{T}"/> keeps its elements in comparer order, so it can answer
    /// <c>Min</c> / <c>Max</c> in O(log n) and hand back a live
    /// <see cref="SortedSet{T}.GetViewBetween(T, T)"/> range view. The trade is that add,
    /// remove, and contains are O(log n) rather than <see cref="HashSet{T}"/>'s O(1).
    /// </summary>
    public static Task RunSortedSetAsync()
    {
        var set = new SortedSet<int> { 42, 7, 19, 3, 25, 11, 33 };
        Console.WriteLine($"insertion order was 42,7,19,3,25,11,33; enumeration is sorted: {string.Join(", ", set)}");
        Console.WriteLine($"Min = {set.Min}, Max = {set.Max}");

        var mid = set.GetViewBetween(10, 30);
        Console.WriteLine($"GetViewBetween(10, 30) = {string.Join(", ", mid)}");

        set.Add(20);
        Console.WriteLine($"after Add(20), the view reflects it live: {string.Join(", ", mid)}");

        var descending = new SortedSet<int>(set, Comparer<int>.Create((x, y) => y.CompareTo(x)));
        Console.WriteLine($"same elements, reverse comparer: {string.Join(", ", descending)}");

        return Task.CompletedTask;
    }
}

/* Expected output (RunOperationsAsync)
a = {1, 2, 3, 4, 5}, b = {4, 5, 6, 7}
  union           : mutating -> {1, 2, 3, 4, 5, 6, 7}, LINQ -> {1, 2, 3, 4, 5, 6, 7} [agree]
  intersect       : mutating -> {4, 5}, LINQ -> {4, 5} [agree]
  except          : mutating -> {1, 2, 3}, LINQ -> {1, 2, 3} [agree]
  symmetric except: mutating -> {1, 2, 3, 6, 7}, LINQ -> {1, 2, 3, 6, 7} [agree]
*/

/* Expected output (RunRelationsAsync)
evens.IsSubsetOf(universe)          = True
evens.IsProperSubsetOf(universe)    = True
universe.IsSubsetOf(universe)       = True
universe.IsProperSubsetOf(universe) = False
withSix.IsSupersetOf(universe)      = True
evens.Overlaps(universe)            = True
empty.IsSubsetOf(universe)          = True
empty.IsProperSubsetOf(universe)    = True
empty.Overlaps(universe)            = False
universe.SetEquals([5, 4, 3, 2, 1]) = True
*/

/* Expected output (RunSortedSetAsync)
insertion order was 42,7,19,3,25,11,33; enumeration is sorted: 3, 7, 11, 19, 25, 33, 42
Min = 3, Max = 42
GetViewBetween(10, 30) = 11, 19, 25
after Add(20), the view reflects it live: 11, 19, 20, 25
same elements, reverse comparer: 42, 33, 25, 20, 19, 11, 7, 3
*/
