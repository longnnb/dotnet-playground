using System.Collections.Frozen;
using System.Diagnostics;

namespace CollectionTypes;

/// <summary>
/// Measures what the collection choice actually costs: membership tests across
/// <see cref="List{T}"/>, <see cref="HashSet{T}"/>, <see cref="Dictionary{TKey, TValue}"/> and
/// <see cref="FrozenSet{T}"/>, and the allocation difference between building a large collection
/// pre-sized versus letting it grow.
/// </summary>
public static class LookupPerformanceDemo
{
    private const int N = 100_000;

    /// <summary>
    /// Times <c>N</c> negative lookups (worst case: the item is absent, so a
    /// <see cref="List{T}"/> scans everything). The hash-based structures should be
    /// dramatically faster; the exact ratio varies by machine, so this asserts an invariant
    /// rather than pinning numbers.
    /// </summary>
    public static Task RunLookupPerformanceAsync()
    {
        var values = Enumerable.Range(0, N).ToArray();
        var list = new List<int>(values);
        var hashSet = new HashSet<int>(values);
        var dictionary = values.ToDictionary(v => v, v => v);
        var frozenSet = values.ToFrozenSet();

        int[] probes = [.. Enumerable.Range(N, 2_000)]; // all absent

        var listMs = Time(() => probes.Count(list.Contains));
        var hashMs = Time(() => probes.Count(hashSet.Contains));
        var dictMs = Time(() => probes.Count(dictionary.ContainsKey));
        var frozenMs = Time(() => probes.Count(frozenSet.Contains));

        Console.WriteLine($"{probes.Length} absent-key probes against {N:N0} items:");
        Console.WriteLine($"  List<int>.Contains      {listMs,8:F2} ms");
        Console.WriteLine($"  HashSet<int>.Contains   {hashMs,8:F2} ms");
        Console.WriteLine($"  Dictionary.ContainsKey  {dictMs,8:F2} ms");
        Console.WriteLine($"  FrozenSet<int>.Contains {frozenMs,8:F2} ms");

        var invariant = hashMs * 20 < listMs;
        Console.WriteLine($"  HashSet at least 20x faster than List: {invariant} [{(invariant ? "holds" : "VIOLATED")}]");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Building a collection to a known final size in one allocation
    /// (<c>new List&lt;int&gt;(capacity)</c>, <c>new Dictionary&lt;,&gt;(capacity)</c>) skips
    /// the doubling-and-copy churn of letting it grow from empty.
    /// <see cref="GC.GetAllocatedBytesForCurrentThread"/> makes the difference visible; it is
    /// far steadier between runs than wall-clock time.
    /// </summary>
    public static Task RunPresizeVsGrowAsync()
    {
        var grownListBytes = Measure(() =>
        {
            var l = new List<int>();
            for (var i = 0; i < N; i++)
            {
                l.Add(i);
            }
        });

        var presizedListBytes = Measure(() =>
        {
            var l = new List<int>(N);
            for (var i = 0; i < N; i++)
            {
                l.Add(i);
            }
        });

        var grownDictBytes = Measure(() =>
        {
            var d = new Dictionary<int, int>();
            for (var i = 0; i < N; i++)
            {
                d[i] = i;
            }
        });

        var presizedDictBytes = Measure(() =>
        {
            var d = new Dictionary<int, int>(N);
            for (var i = 0; i < N; i++)
            {
                d[i] = i;
            }
        });

        Console.WriteLine($"building {N:N0} entries:");
        Console.WriteLine($"  List grown from empty:   {grownListBytes,10:N0} bytes");
        Console.WriteLine($"  List pre-sized:          {presizedListBytes,10:N0} bytes");
        Console.WriteLine($"  Dictionary grown:        {grownDictBytes,10:N0} bytes");
        Console.WriteLine($"  Dictionary pre-sized:    {presizedDictBytes,10:N0} bytes");

        var listOk = presizedListBytes < grownListBytes;
        var dictOk = presizedDictBytes < grownDictBytes;
        Console.WriteLine($"  pre-sizing allocates less for List:       {listOk} [{(listOk ? "holds" : "VIOLATED")}]");
        Console.WriteLine($"  pre-sizing allocates less for Dictionary: {dictOk} [{(dictOk ? "holds" : "VIOLATED")}]");

        return Task.CompletedTask;
    }

    private static double Time(Func<int> action)
    {
        action(); // warm up JIT and caches
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 20; i++)
        {
            _ = action();
        }

        sw.Stop();
        return sw.Elapsed.TotalMilliseconds / 20;
    }

    private static long Measure(Action build)
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        build();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}

/* Expected output (RunLookupPerformanceAsync) -- timings vary widely by machine and build
   configuration; the numbers below are one Debug run. The invariant is that HashSet is at
   least 20x faster than List for absent-key probes.
2000 absent-key probes against 100,000 items:
  List<int>.Contains         15.21 ms
  HashSet<int>.Contains       0.03 ms
  Dictionary.ContainsKey      0.03 ms
  FrozenSet<int>.Contains     0.03 ms
  HashSet at least 20x faster than List: True [holds]
*/

/* Expected output (RunPresizeVsGrowAsync) -- byte counts vary a little between runtimes; the
   invariants are that pre-sizing allocates strictly less for both List and Dictionary
building 100,000 entries:
  List grown from empty:    1,048,976 bytes
  List pre-sized:             400,056 bytes
  Dictionary grown:         6,037,640 bytes
  Dictionary pre-sized:     2,172,752 bytes
  pre-sizing allocates less for List:       True [holds]
  pre-sizing allocates less for Dictionary: True [holds]
*/
