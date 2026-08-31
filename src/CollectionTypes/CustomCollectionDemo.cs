using System.Collections;

namespace CollectionTypes;

/// <summary>
/// Demonstrates writing and consuming custom sequences: the deferred, re-runnable nature of a
/// <c>yield return</c> iterator, a hand-rolled <see cref="IEnumerable{T}"/> with an indexer, and
/// the difference between <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/> (which
/// may enumerate) and
/// <see cref="Enumerable.TryGetNonEnumeratedCount{TSource}(IEnumerable{TSource}, out int)"/>
/// (which never does).
/// </summary>
public static class CustomCollectionDemo
{
    /// <summary>
    /// A <c>yield</c> iterator does nothing when called -- the body first runs on the opening
    /// <c>MoveNext</c>. Each fresh <c>foreach</c> runs it again from the top, and it reads any
    /// captured variable at iteration time, not at call time.
    /// </summary>
    public static Task RunYieldDeferredExecutionAsync()
    {
        var source = new List<int> { 1, 2, 3 };
        var log = new List<string>();

        IEnumerable<int> DoubleEach()
        {
            log.Add("iterator body started");
            foreach (var n in source)
            {
                yield return n * 2;
            }
        }

        var query = DoubleEach();
        Console.WriteLine($"after calling the iterator, log entries: {log.Count} (body has not run)");

        source.Add(4);
        Console.WriteLine($"first enumeration (source is now 1,2,3,4): [{string.Join(", ", query)}]");

        source.Insert(0, 0);
        Console.WriteLine($"second enumeration (source is now 0,1,2,3,4): [{string.Join(", ", query)}]");
        Console.WriteLine($"iterator body ran {log.Count} times total -- once per enumeration, each time re-reading the source");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Implementing <see cref="IEnumerable{T}"/> is enough for <c>foreach</c> and LINQ; adding an
    /// indexer gives positional access; implementing <see cref="ICollection{T}"/> (mutators
    /// throwing, as arrays do) advertises an O(1) <see cref="ICollection{T}.Count"/> that LINQ's
    /// <c>Count()</c> fast path can use -- that path checks for <see cref="ICollection{T}"/>, not
    /// <see cref="IReadOnlyCollection{T}"/>. (The <c>count-vs-count</c> demo measures that.)
    /// </summary>
    public static Task RunCustomEnumerableAsync()
    {
        var days = new CircularSequence<string>(["Mon", "Tue", "Wed"]);

        Console.WriteLine($"foreach over 7 positions: {string.Join(", ", Enumerable.Range(0, 7).Select(i => days[i]))}");
        Console.WriteLine($"days[10] wraps to {days[10]}");
        Console.WriteLine($"days[-1] wraps to {days[-1]}");
        Console.WriteLine($"LINQ works: uppercased = [{string.Join(", ", days.Select(d => d.ToUpperInvariant()))}]");
        Console.WriteLine($"days.Count (O(1) property from ICollection<T>) = {days.Count}");
        Console.WriteLine($"it is read-only: IsReadOnly = {((ICollection<string>)days).IsReadOnly}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/> takes an O(1) shortcut for
    /// anything that is an <see cref="ICollection{T}"/> / <see cref="IReadOnlyCollection{T}"/>,
    /// and otherwise walks the whole sequence.
    /// <see cref="Enumerable.TryGetNonEnumeratedCount{TSource}(IEnumerable{TSource}, out int)"/>
    /// only ever takes the shortcut, reporting failure rather than enumerating.
    /// </summary>
    public static Task RunCountVsCountAsync()
    {
        var backedByList = new CountingSequence<int>([10, 20, 30, 40], materialiseAsList: true);
        Console.WriteLine("-- sequence backed by a List<int> --");
        Console.WriteLine($"TryGetNonEnumeratedCount -> {backedByList.Source.TryGetNonEnumeratedCount(out var fast)} (count {fast})");
        Console.WriteLine($"items touched by that call: {backedByList.ItemsTouched}");
        _ = backedByList.Source.Count();
        Console.WriteLine($"items touched after Count(): {backedByList.ItemsTouched} (shortcut, no walk)");

        var lazyPipeline = new CountingSequence<int>([10, 20, 30, 40], materialiseAsList: false);
        Console.WriteLine("-- same values behind a Where(...) pipeline --");
        Console.WriteLine($"TryGetNonEnumeratedCount -> {lazyPipeline.Source.Where(_ => true).TryGetNonEnumeratedCount(out var maybe)} (count {maybe})");
        var counted = lazyPipeline.Source.Where(_ => true).Count();
        Console.WriteLine($"Count() returned {counted}; items touched: {lazyPipeline.ItemsTouched} (had to walk every one)");

        return Task.CompletedTask;
    }
}

/// <summary>
/// An index-addressable sequence that wraps out-of-range indices, over a fixed set of items.
/// Implements <see cref="ICollection{T}"/> read-only -- every mutating member throws, the way
/// <see cref="Array"/>'s <see cref="IList"/> implementation does.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
file sealed class CircularSequence<T>(IReadOnlyList<T> items) : ICollection<T>
{
    private readonly IReadOnlyList<T> _items = items;

    /// <inheritdoc/>
    public int Count => _items.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => true;

    /// <summary>Gets the element at <paramref name="index"/> modulo the item count.</summary>
    /// <param name="index">Any integer; negative and out-of-range values wrap.</param>
    public T this[int index] => _items[((index % _items.Count) + _items.Count) % _items.Count];

    /// <inheritdoc/>
    public bool Contains(T item) => _items.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        for (var i = 0; i < _items.Count; i++)
        {
            array[arrayIndex + i] = _items[i];
        }
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public void Add(T item) => throw new NotSupportedException("CircularSequence is read-only.");

    /// <inheritdoc/>
    public void Clear() => throw new NotSupportedException("CircularSequence is read-only.");

    /// <inheritdoc/>
    public bool Remove(T item) => throw new NotSupportedException("CircularSequence is read-only.");
}

/// <summary>
/// Wraps a set of values as either a materialised <see cref="List{T}"/> or a lazy
/// <see cref="IEnumerable{T}"/>, counting how many elements enumeration actually pulls.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
file sealed class CountingSequence<T>
{
    private readonly List<T> _values;
    private readonly bool _materialiseAsList;

    /// <summary>Creates the wrapper.</summary>
    /// <param name="values">The backing values.</param>
    /// <param name="materialiseAsList">
    /// <see langword="true"/> to expose a real <see cref="List{T}"/>; <see langword="false"/> to
    /// expose a counting iterator.
    /// </param>
    public CountingSequence(IEnumerable<T> values, bool materialiseAsList)
    {
        _values = [.. values];
        _materialiseAsList = materialiseAsList;
    }

    /// <summary>How many elements have been pulled through the iterator so far.</summary>
    public int ItemsTouched { get; private set; }

    /// <summary>The sequence to query -- a <see cref="List{T}"/> or a counting iterator.</summary>
    public IEnumerable<T> Source => _materialiseAsList ? _values : Iterate();

    private IEnumerable<T> Iterate()
    {
        foreach (var value in _values)
        {
            ItemsTouched++;
            yield return value;
        }
    }
}

/* Expected output (RunYieldDeferredExecutionAsync)
after calling the iterator, log entries: 0 (body has not run)
first enumeration (source is now 1,2,3,4): [2, 4, 6, 8]
second enumeration (source is now 0,1,2,3,4): [0, 2, 4, 6, 8]
iterator body ran 2 times total -- once per enumeration, each time re-reading the source
*/

/* Expected output (RunCustomEnumerableAsync)
foreach over 7 positions: Mon, Tue, Wed, Mon, Tue, Wed, Mon
days[10] wraps to Tue
days[-1] wraps to Wed
LINQ works: uppercased = [MON, TUE, WED]
days.Count (O(1) property from ICollection<T>) = 3
it is read-only: IsReadOnly = True
*/

/* Expected output (RunCountVsCountAsync)
-- sequence backed by a List<int> --
TryGetNonEnumeratedCount -> True (count 4)
items touched by that call: 0
items touched after Count(): 0 (shortcut, no walk)
-- same values behind a Where(...) pipeline --
TryGetNonEnumeratedCount -> False (count 0)
Count() returned 4; items touched: 4 (had to walk every one)
*/
