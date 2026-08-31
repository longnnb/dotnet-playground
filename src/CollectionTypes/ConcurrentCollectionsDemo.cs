using System.Collections.Concurrent;

namespace CollectionTypes;

/// <summary>
/// Demonstrates <see cref="System.Collections.Concurrent"/>: why
/// <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/>'s value
/// factory can run more than once, how <c>AddOrUpdate</c> makes a lost-update-free counter,
/// why <c>Count</c> / <c>IsEmpty</c> on a concurrent queue are only snapshots, and the bounded
/// producer/consumer handoff of <see cref="BlockingCollection{T}"/>.
/// </summary>
/// <remarks>
/// This is the collection-type view of concurrency; the locking primitives themselves
/// (<c>lock</c>, <c>SemaphoreSlim</c>, reset events) live in the <c>ThreadSynchronization</c>
/// project.
/// </remarks>
public static class ConcurrentCollectionsDemo
{
    /// <summary>
    /// <c>GetOrAdd(key, factory)</c> guarantees every caller sees one consistent stored value,
    /// but it does <b>not</b> guarantee the factory runs only once -- under contention several
    /// threads can invoke it before any of them publishes. Wrapping the value in
    /// <see cref="Lazy{T}"/> moves the run-once guarantee to where an expensive or
    /// side-effecting factory needs it.
    /// </summary>
    public static async Task RunGetOrAddAsync()
    {
        var factoryRuns = 0;
        var dict = new ConcurrentDictionary<string, int>();

        await RunConcurrently(64, () =>
        {
            dict.GetOrAdd("key", _ =>
            {
                Interlocked.Increment(ref factoryRuns);
                Thread.SpinWait(2_000); // widen the race window
                return 42;
            });
        });

        Console.WriteLine($"plain GetOrAdd: stored value = {dict["key"]} (always 42), factory ran {factoryRuns} time(s)");
        Console.WriteLine($"  factory ran more than once: {factoryRuns > 1} (nondeterministic; the stored value is still consistent)");

        var lazyFactoryRuns = 0;
        var lazyDict = new ConcurrentDictionary<string, Lazy<int>>();

        await RunConcurrently(64, () =>
        {
            var lazy = lazyDict.GetOrAdd("key", _ => new Lazy<int>(() =>
            {
                Interlocked.Increment(ref lazyFactoryRuns);
                Thread.SpinWait(2_000);
                return 42;
            }));
            _ = lazy.Value;
        });

        Console.WriteLine($"Lazy<int> value: {lazyDict["key"].Value}, inner factory ran {lazyFactoryRuns} time(s) [{(lazyFactoryRuns == 1 ? "exactly once" : "UNEXPECTED")}]");
    }

    /// <summary>
    /// <c>AddOrUpdate</c> applies its update delegate atomically per key (retrying on
    /// contention), so N tasks each incrementing reach exactly N. The same
    /// read-then-write done by hand on a plain <see cref="Dictionary{TKey, TValue}"/> value
    /// loses updates to the race between the read and the write.
    /// </summary>
    /// <remarks>
    /// The plain-dictionary side keeps to a single pre-inserted key so the race is confined to
    /// one value slot and cannot corrupt the dictionary's structure or hang the demo.
    /// </remarks>
    public static async Task RunAddOrUpdateAsync()
    {
        const int workers = 1_000;

        var safe = new ConcurrentDictionary<string, int>();
        await RunConcurrently(workers, () => safe.AddOrUpdate("hits", 1, (_, current) => current + 1));

        var racy = new Dictionary<string, int> { ["hits"] = 0 };
        try
        {
            await RunConcurrently(workers, () => racy["hits"] = racy["hits"] + 1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  plain dictionary threw {ex.GetType().Name}");
        }

        Console.WriteLine($"ConcurrentDictionary.AddOrUpdate: hits = {safe["hits"]} of {workers} [{(safe["hits"] == workers ? "exact" : "LOST UPDATES")}]");
        Console.WriteLine($"plain Dictionary read-modify-write: hits = {racy["hits"]} of {workers} (typically < {workers}: lost updates)");
    }

    /// <summary>
    /// <see cref="ConcurrentQueue{T}"/> hands off items safely, but
    /// <see cref="ConcurrentQueue{T}.Count"/> and <c>IsEmpty</c> are point-in-time snapshots --
    /// by the time you act on one it may already be stale. Drain with a
    /// <c>while (TryDequeue(...))</c> loop, not a <c>Count</c>-driven <c>for</c>.
    /// </summary>
    public static async Task RunConcurrentQueueAsync()
    {
        var queue = new ConcurrentQueue<int>();
        const int perProducer = 500;
        const int producers = 4;

        var production = Enumerable.Range(0, producers).Select(p => Task.Run(() =>
        {
            for (var i = 0; i < perProducer; i++)
            {
                queue.Enqueue(p * perProducer + i);
            }
        }));

        var consumed = 0;
        var consumer = Task.Run(async () =>
        {
            var idleRounds = 0;
            while (idleRounds < 100)
            {
                if (queue.TryDequeue(out _))
                {
                    Interlocked.Increment(ref consumed);
                    idleRounds = 0;
                }
                else
                {
                    idleRounds++;
                    await Task.Yield();
                }
            }
        });

        await Task.WhenAll(production);
        await consumer;

        Console.WriteLine($"produced {producers * perProducer}, consumed {consumed} [{(consumed == producers * perProducer ? "all accounted for" : "MISMATCH")}]");
        Console.WriteLine($"queue.IsEmpty = {queue.IsEmpty}, queue.Count = {queue.Count} (a snapshot -- fine here only because everything has joined)");
    }

    /// <summary>
    /// A bounded <see cref="BlockingCollection{T}"/> blocks the producer when it is full and the
    /// consumer when it is empty. The producer calls
    /// <see cref="BlockingCollection{T}.CompleteAdding"/> when done, which ends the consumer's
    /// <see cref="BlockingCollection{T}.GetConsumingEnumerable()"/> loop.
    /// </summary>
    /// <remarks>
    /// For an <see langword="async"/> pipeline prefer <c>System.Threading.Channels.Channel&lt;T&gt;</c>:
    /// same bounded handoff, but the waits are <c>await</c>s rather than blocked threads.
    /// </remarks>
    public static async Task RunBlockingCollectionAsync()
    {
        using var buffer = new BlockingCollection<int>(boundedCapacity: 4);
        const int total = 20;
        var maxObservedCount = 0;

        var producer = Task.Run(() =>
        {
            for (var i = 1; i <= total; i++)
            {
                buffer.Add(i);
                maxObservedCount = Math.Max(maxObservedCount, buffer.Count);
            }

            buffer.CompleteAdding();
        });

        var sum = 0;
        var consumer = Task.Run(() =>
        {
            foreach (var item in buffer.GetConsumingEnumerable())
            {
                sum += item;
                Thread.SpinWait(5_000); // consumer slower than producer, so the buffer fills
            }
        });

        await Task.WhenAll(producer, consumer);

        Console.WriteLine($"consumed {total} items, sum = {sum} [{(sum == total * (total + 1) / 2 ? "correct" : "WRONG")}]");
        Console.WriteLine($"buffer never exceeded its bound of 4: {maxObservedCount <= 4} [{(maxObservedCount <= 4 ? "holds" : "VIOLATED")}] (peak observed {maxObservedCount})");
    }

    private static Task RunConcurrently(int count, Action action) =>
        Task.WhenAll(Enumerable.Range(0, count).Select(_ => Task.Run(action)));
}

/* Expected output (RunGetOrAddAsync) -- nondeterministic: the factory run count depends on how
   the 64 tasks race. It is > 1 on any multi-core machine (one sample below); on a single core
   it can be 1. The stored value is always 42, and the Lazy<int> factory always runs exactly once.
plain GetOrAdd: stored value = 42 (always 42), factory ran 4 time(s)
  factory ran more than once: True (nondeterministic; the stored value is still consistent)
Lazy<int> value: 42, inner factory ran 1 time(s) [exactly once]
*/

/* Expected output (RunAddOrUpdateAsync) -- nondeterministic: AddOrUpdate always reaches 1000;
   the plain-dictionary count is whatever the lost-update race leaves (one sample below), always
   <= 1000 and almost always < 1000
ConcurrentDictionary.AddOrUpdate: hits = 1000 of 1000 [exact]
plain Dictionary read-modify-write: hits = 941 of 1000 (typically < 1000: lost updates)
*/

/* Expected output (RunConcurrentQueueAsync)
produced 2000, consumed 2000 [all accounted for]
queue.IsEmpty = True, queue.Count = 0 (a snapshot -- fine here only because everything has joined)
*/

/* Expected output (RunBlockingCollectionAsync) -- the peak observed count can be anywhere from
   1 to 4 depending on scheduling; the invariant is that it never exceeds the bound of 4
consumed 20 items, sum = 210 [correct]
buffer never exceeded its bound of 4: True [holds] (peak observed 4)
*/
