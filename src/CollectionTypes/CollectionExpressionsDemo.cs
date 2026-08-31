using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace CollectionTypes;

/// <summary>
/// Demonstrates C# 12-13 collection expressions: the target-typed <c>[...]</c> literal and its
/// spread element, <c>params</c> <see cref="ReadOnlySpan{T}"/> versus <c>params T[]</c>, and
/// <see cref="CollectionBuilderAttribute"/> making a custom type constructible from <c>[...]</c>.
/// </summary>
public static class CollectionExpressionsDemo
{
    /// <summary>
    /// One <c>[1, 2, 3]</c> literal compiles differently per target type -- a bare array, a
    /// <see cref="List{T}"/>, a stack-allocated <see cref="Span{T}"/>, an
    /// <see cref="ImmutableArray{T}"/> -- and the spread element <c>..other</c> inlines another
    /// sequence's items.
    /// </summary>
    public static Task RunExpressionsAsync()
    {
        int[] asArray = [1, 2, 3];
        List<int> asList = [1, 2, 3];
        Span<int> asSpan = [1, 2, 3];
        ImmutableArray<int> asImmutable = [1, 2, 3];
        int[] empty = [];

        Console.WriteLine($"array      -> {asArray.GetType().Name}, [{string.Join(", ", asArray)}]");
        Console.WriteLine($"list       -> {asList.GetType().Name}, [{string.Join(", ", asList)}]");
        Console.WriteLine($"span       -> length {asSpan.Length}, [{string.Join(", ", asSpan.ToArray())}]");
        Console.WriteLine($"immutable  -> {asImmutable.GetType().Name}, [{string.Join(", ", asImmutable)}]");
        Console.WriteLine($"empty      -> length {empty.Length}");

        int[] head = [1, 2];
        int[] tail = [5, 6];
        int[] spread = [..head, 3, 4, ..tail];
        Console.WriteLine($"[..head, 3, 4, ..tail] -> [{string.Join(", ", spread)}]");

        return Task.CompletedTask;
    }

    /// <summary>
    /// When both a <c>params ReadOnlySpan&lt;T&gt;</c> and a <c>params T[]</c> overload are in
    /// scope, a call with inline arguments binds to the span one -- the compiler can satisfy it
    /// from a stack buffer, allocating no array.
    /// </summary>
    public static Task RunParamsCollectionsAsync()
    {
        Console.WriteLine($"Sum(1, 2, 3, 4) = {Sum(1, 2, 3, 4)} (bound to the ReadOnlySpan overload)");

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++)
        {
            _ = Sum(i, i + 1, i + 2);
        }

        var spanBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 1000; i++)
        {
            _ = SumArray(i, i + 1, i + 2);
        }

        var arrayBytes = GC.GetAllocatedBytesForCurrentThread() - before;

        Console.WriteLine($"1000 calls, params ReadOnlySpan<int>: ~{spanBytes:N0} bytes allocated");
        Console.WriteLine($"1000 calls, params int[]:            ~{arrayBytes:N0} bytes allocated");
        Console.WriteLine($"span overload allocates less: {spanBytes < arrayBytes} [{(spanBytes < arrayBytes ? "holds" : "VIOLATED")}]");

        return Task.CompletedTask;
    }

    private static int Sum(params ReadOnlySpan<int> values)
    {
        var total = 0;
        foreach (var value in values)
        {
            total += value;
        }

        return total;
    }

    private static int SumArray(params int[] values)
    {
        var total = 0;
        foreach (var value in values)
        {
            total += value;
        }

        return total;
    }

    /// <summary>
    /// A type marked <c>[CollectionBuilder(typeof(Builder), "Create")]</c> with a builder method
    /// taking a <see cref="ReadOnlySpan{T}"/> can be constructed straight from a <c>[...]</c>
    /// literal, spreads included.
    /// </summary>
    public static Task RunCollectionBuilderAsync()
    {
        RingBuffer<string> ring = ["oldest", "middle", "newest"];
        Console.WriteLine($"RingBuffer<string> from [...]: {ring}");

        string[] more = ["x", "y"];
        RingBuffer<string> spread = [..more, "z"];
        Console.WriteLine($"RingBuffer<string> from [..more, \"z\"]: {spread}");

        return Task.CompletedTask;
    }
}

/// <summary>Factory for <see cref="RingBuffer{T}"/> collection expressions.</summary>
file static class RingBufferBuilder
{
    /// <summary>Builds a <see cref="RingBuffer{T}"/> from the elements of a collection expression.</summary>
    public static RingBuffer<T> Create<T>(ReadOnlySpan<T> items) => new(items);
}

/// <summary>A tiny fixed-content sequence, constructible from a collection expression.</summary>
/// <typeparam name="T">Element type.</typeparam>
[CollectionBuilder(typeof(RingBufferBuilder), nameof(RingBufferBuilder.Create))]
file sealed class RingBuffer<T>(ReadOnlySpan<T> items) : IEnumerable<T>
{
    private readonly T[] _items = items.ToArray();

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    /// <summary>Renders the buffer as <c>[a, b, c]</c>.</summary>
    public override string ToString() => $"[{string.Join(", ", _items)}]";
}

/* Expected output (RunExpressionsAsync)
array      -> Int32[], [1, 2, 3]
list       -> List`1, [1, 2, 3]
span       -> length 3, [1, 2, 3]
immutable  -> ImmutableArray`1, [1, 2, 3]
empty      -> length 0
[..head, 3, 4, ..tail] -> [1, 2, 3, 4, 5, 6]
*/

/* Expected output (RunParamsCollectionsAsync) -- byte counts are approximate; the invariant is
   that the ReadOnlySpan overload allocates less (it allocates nothing here)
Sum(1, 2, 3, 4) = 10 (bound to the ReadOnlySpan overload)
1000 calls, params ReadOnlySpan<int>: ~0 bytes allocated
1000 calls, params int[]:            ~40,000 bytes allocated
span overload allocates less: True [holds]
*/

/* Expected output (RunCollectionBuilderAsync)
RingBuffer<string> from [...]: [oldest, middle, newest]
RingBuffer<string> from [..more, "z"]: [x, y, z]
*/
