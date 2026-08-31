using System.Runtime.InteropServices;

namespace CollectionTypes;

/// <summary>
/// Demonstrates <see cref="List{T}"/>: how its backing array grows, the classic
/// remove-while-iterating bug and three fixes that actually agree, the
/// <see cref="InvalidOperationException"/> from structurally mutating a list mid-<c>foreach</c>,
/// the difference between <see cref="List{T}.Sort()"/> (unstable) and LINQ
/// <see cref="Enumerable.OrderBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})"/>
/// (stable), and <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/> for in-place element
/// mutation.
/// </summary>
public static class ListDemo
{
    /// <summary>
    /// <see cref="List{T}.Capacity"/> (allocated slots) and <see cref="List{T}.Count"/> (used
    /// slots) are different numbers: capacity doubles from 4 as the list fills, independent of
    /// how many elements you have.
    /// </summary>
    /// <remarks>
    /// <see cref="List{T}.EnsureCapacity(int)"/> grows the array in one shot if you know the
    /// final size; <see cref="List{T}.TrimExcess()"/> shrinks it back down to <c>Count</c> once
    /// you are done adding. Removing elements never shrinks the backing array on its own.
    /// </remarks>
    public static Task RunGrowthAsync()
    {
        var list = new List<int>();
        var capacity = list.Capacity;
        Console.WriteLine($"new List<int>(): Count={list.Count}, Capacity={capacity}");

        for (var i = 1; i <= 20; i++)
        {
            list.Add(i);

            if (list.Capacity != capacity)
            {
                capacity = list.Capacity;
                Console.WriteLine($"after adding {i,2}: Count={list.Count,2}, Capacity grew to {capacity}");
            }
        }

        list.Clear();
        Console.WriteLine($"after Clear(): Count={list.Count}, Capacity still {list.Capacity}");

        list.TrimExcess();
        Console.WriteLine($"after TrimExcess(): Count={list.Count}, Capacity={list.Capacity}");

        var presized = new List<int>();
        presized.EnsureCapacity(1000);
        Console.WriteLine($"EnsureCapacity(1000): Count={presized.Count}, Capacity={presized.Capacity}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removing matched elements in a forward <c>for</c> loop skips the element that shifts into
    /// the freed slot. Iterating backwards, or using
    /// <see cref="List{T}.RemoveAll(Predicate{T})"/>, does not.
    /// </summary>
    public static Task RunRemovalAsync()
    {
        int[] source = [1, 2, 4, 4, 6, 4, 7, 8];
        Console.WriteLine($"source: [{string.Join(", ", source)}] -- remove every even number");

        var buggy = RemoveEvensForwardByIndex([.. source]);
        var backward = RemoveEvensBackwardByIndex([.. source]);
        var removeAll = RemoveEvensWithRemoveAll([.. source]);
        var linq = source.Where(n => n % 2 != 0).ToList();

        Console.WriteLine($"forward-by-index:  [{string.Join(", ", buggy)}]");
        Console.WriteLine($"backward-by-index: [{string.Join(", ", backward)}]");
        Console.WriteLine($"RemoveAll:         [{string.Join(", ", removeAll)}]");
        Console.WriteLine($"Where (reference): [{string.Join(", ", linq)}]");

        var backwardOk = backward.SequenceEqual(linq);
        var removeAllOk = removeAll.SequenceEqual(linq);
        var forwardOk = buggy.SequenceEqual(linq);
        Console.WriteLine(
            $"backward vs Where [{(backwardOk ? "agree" : "DIFFER")}], " +
            $"RemoveAll vs Where [{(removeAllOk ? "agree" : "DIFFER")}], " +
            $"forward vs Where [{(forwardOk ? "agree" : "DIFFER")}]");

        return Task.CompletedTask;
    }

    private static List<int> RemoveEvensForwardByIndex(List<int> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] % 2 == 0)
            {
                list.RemoveAt(i);
            }
        }

        return list;
    }

    private static List<int> RemoveEvensBackwardByIndex(List<int> list)
    {
        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] % 2 == 0)
            {
                list.RemoveAt(i);
            }
        }

        return list;
    }

    private static List<int> RemoveEvensWithRemoveAll(List<int> list)
    {
        list.RemoveAll(n => n % 2 == 0);
        return list;
    }

    /// <summary>
    /// A <c>foreach</c> over a <see cref="List{T}"/> takes a version stamp on entry; any
    /// structural change (<c>Add</c>, <c>Remove</c>, <c>Clear</c>) during iteration makes the
    /// next <c>MoveNext</c> throw <see cref="InvalidOperationException"/>. Reassigning an
    /// existing slot by index does not.
    /// </summary>
    public static Task RunMutationDuringEnumerationAsync()
    {
        var list = new List<int> { 1, 2, 3 };

        try
        {
            foreach (var n in list)
            {
                Console.WriteLine($"visiting {n}");

                if (n == 2)
                {
                    list.Add(99);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"caught {ex.GetType().Name}: {ex.Message}");
        }

        list = [1, 2, 3];
        for (var i = 0; i < list.Count; i++)
        {
            list[i] *= 10;
        }

        Console.WriteLine($"reassigning slots by index is fine: [{string.Join(", ", list)}]");

        return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="List{T}.Sort()"/> is an introsort: fast, in place, and <b>not stable</b> --
    /// elements with equal keys can be reordered. LINQ <c>OrderBy</c> is a stable merge sort and
    /// preserves their original order. <see cref="List{T}.BinarySearch(T)"/> returns the
    /// bitwise complement of the insertion point when the item is absent.
    /// </summary>
    /// <remarks>
    /// Instability only shows up once introsort actually partitions (above its ~16-element
    /// insertion-sort threshold), so this uses 60 items each tagged with its original position
    /// and sorts them by <c>value % 4</c>. "Stable" means those tags stay ascending inside every
    /// equal-key group.
    /// </remarks>
    public static Task RunSortAndSearchAsync()
    {
        var items = Enumerable.Range(0, 60)
            .Select(i => (Original: i, Key: (i * 7) % 4))
            .ToArray();

        var byListSort = items.ToList();
        byListSort.Sort((x, y) => x.Key.CompareTo(y.Key));

        var byOrderBy = items.OrderBy(p => p.Key).ToList();

        Console.WriteLine($"List.Sort by (value % 4) is stable this run: {IsStable(byListSort)} [{(IsStable(byListSort) ? "agree" : "DIFFER")}]");
        Console.WriteLine($"OrderBy by (value % 4) is stable:            {IsStable(byOrderBy)} [{(IsStable(byOrderBy) ? "agree" : "DIFFER")}]");

        var sorted = new List<int> { 10, 20, 30, 40, 50 };
        var found = sorted.BinarySearch(30);
        var missing = sorted.BinarySearch(35);
        Console.WriteLine($"BinarySearch(30) = {found} (index)");
        Console.WriteLine($"BinarySearch(35) = {missing}, so insertion point = ~{missing} = {~missing}");

        return Task.CompletedTask;
    }

    private static bool IsStable(List<(int Original, int Key)> sorted)
    {
        for (var i = 1; i < sorted.Count; i++)
        {
            if (sorted[i].Key == sorted[i - 1].Key && sorted[i].Original < sorted[i - 1].Original)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// <see cref="CollectionsMarshal.AsSpan{T}(List{T}?)"/> hands you a <see cref="Span{T}"/>
    /// over the list's live backing array -- writing through it mutates the list with no
    /// per-element bounds check and no copy.
    /// </summary>
    /// <remarks>
    /// The span is invalidated by any structural change to the list (anything that could
    /// reallocate or move the backing array). Finish with the span before you add to or remove
    /// from the list.
    /// </remarks>
    public static Task RunAsSpanAsync()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var span = CollectionsMarshal.AsSpan(list);

        for (var i = 0; i < span.Length; i++)
        {
            span[i] *= span[i];
        }

        Console.WriteLine($"squared in place via AsSpan: [{string.Join(", ", list)}]");

        return Task.CompletedTask;
    }
}

/* Expected output (RunGrowthAsync) -- Capacity values are the current runtime's growth policy
   (double from 4); a future runtime could pick different steps
new List<int>(): Count=0, Capacity=0
after adding  1: Count= 1, Capacity grew to 4
after adding  5: Count= 5, Capacity grew to 8
after adding  9: Count= 9, Capacity grew to 16
after adding 17: Count=17, Capacity grew to 32
after Clear(): Count=0, Capacity still 32
after TrimExcess(): Count=0, Capacity=0
EnsureCapacity(1000): Count=0, Capacity=1000
*/

/* Expected output (RunRemovalAsync)
source: [1, 2, 4, 4, 6, 4, 7, 8] -- remove every even number
forward-by-index:  [1, 4, 6, 7]
backward-by-index: [1, 7]
RemoveAll:         [1, 7]
Where (reference): [1, 7]
backward vs Where [agree], RemoveAll vs Where [agree], forward vs Where [DIFFER]
*/

/* Expected output (RunMutationDuringEnumerationAsync)
visiting 1
visiting 2
caught InvalidOperationException: Collection was modified; enumeration operation may not execute.
reassigning slots by index is fine: [10, 20, 30]
*/

/* Expected output (RunSortAndSearchAsync) -- "stable this run" for List.Sort is whatever the
   current runtime's introsort happens to do with this input; it is deterministic per runtime
   but not contractual. OrderBy is always stable.
List.Sort by (value % 4) is stable this run: False [DIFFER]
OrderBy by (value % 4) is stable:            True [agree]
BinarySearch(30) = 2 (index)
BinarySearch(35) = -4, so insertion point = ~-4 = 3
*/

/* Expected output (RunAsSpanAsync)
squared in place via AsSpan: [1, 4, 9, 16, 25]
*/
