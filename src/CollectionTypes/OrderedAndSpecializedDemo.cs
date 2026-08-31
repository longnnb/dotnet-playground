namespace CollectionTypes;

/// <summary>
/// Demonstrates the ordered and specialised collections: <see cref="Queue{T}"/> /
/// <see cref="Stack{T}"/> (FIFO / LIFO), <see cref="LinkedList{T}"/> (O(1) splice given a node,
/// O(n) to find one), <see cref="PriorityQueue{TElement, TPriority}"/> (heap-ordered, and
/// <b>not</b> stable across equal priorities), and <see cref="SortedDictionary{TKey, TValue}"/>
/// versus <see cref="SortedList{TKey, TValue}"/>.
/// </summary>
public static class OrderedAndSpecializedDemo
{
    /// <summary>
    /// <see cref="Queue{T}"/> dequeues in insertion order, <see cref="Stack{T}"/> in reverse.
    /// <c>Peek</c> on an empty one throws; <c>TryDequeue</c> / <c>TryPop</c> report emptiness
    /// instead.
    /// </summary>
    public static Task RunQueueAndStackAsync()
    {
        var queue = new Queue<string>();
        foreach (var task in new[] { "build", "test", "deploy" })
        {
            queue.Enqueue(task);
        }

        Console.WriteLine($"queue Peek = {queue.Peek()}");
        Console.WriteLine($"queue drains FIFO: {DrainQueue(queue)}");

        var stack = new Stack<string>();
        foreach (var frame in new[] { "main", "parse", "tokenize" })
        {
            stack.Push(frame);
        }

        Console.WriteLine($"stack Peek = {stack.Peek()}");
        Console.WriteLine($"stack drains LIFO: {DrainStack(stack)}");

        Console.WriteLine($"empty queue TryDequeue = {queue.TryDequeue(out _)}");
        try
        {
            queue.Peek();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"empty queue Peek threw {ex.GetType().Name}");
        }

        return Task.CompletedTask;

        static string DrainQueue(Queue<string> q)
        {
            var items = new List<string>();
            while (q.TryDequeue(out var item))
            {
                items.Add(item);
            }

            return string.Join(" -> ", items);
        }

        static string DrainStack(Stack<string> s)
        {
            var items = new List<string>();
            while (s.TryPop(out var item))
            {
                items.Add(item);
            }

            return string.Join(" -> ", items);
        }
    }

    /// <summary>
    /// <see cref="LinkedList{T}"/> inserts and removes in O(1) <b>once you hold the node</b>, but
    /// <see cref="LinkedList{T}.Find(T)"/> is an O(n) walk. For most workloads a
    /// <see cref="List{T}"/>'s contiguous memory wins despite the O(n) shift, which is why
    /// <c>LinkedList</c> is rarely the right default.
    /// </summary>
    public static Task RunLinkedListAsync()
    {
        var list = new LinkedList<string>(["alpha", "gamma", "delta"]);

        var gamma = list.Find("gamma")!;
        list.AddBefore(gamma, "beta");
        list.AddAfter(gamma, "gamma-and-a-half");

        Console.WriteLine($"after splicing around 'gamma': {string.Join(", ", list)}");

        list.Remove(gamma);
        Console.WriteLine($"after Remove(node for 'gamma'): {string.Join(", ", list)}");

        Console.WriteLine($"First = {list.First!.Value}, Last = {list.Last!.Value}");
        Console.WriteLine($"Find(\"missing\") = {(list.Find("missing") is null ? "null" : "found")}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="PriorityQueue{TElement, TPriority}"/> is a binary heap: <c>Dequeue</c> always
    /// returns a lowest-priority-value item, but among <b>equal</b> priorities the order is
    /// unspecified and generally not insertion order. Encoding a tie-breaker into the priority
    /// (here a <c>(priority, sequence)</c> tuple) makes it deterministic.
    /// </summary>
    public static Task RunPriorityQueueAsync()
    {
        string[] jobs = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j"];

        var naive = new PriorityQueue<string, int>();
        foreach (var job in jobs)
        {
            naive.Enqueue(job, 1);
        }

        var naiveOrder = DrainPq(naive);
        Console.WriteLine($"ten items, all priority 1, dequeue order: {naiveOrder}");
        Console.WriteLine($"  came out in insertion order: {naiveOrder == string.Join(", ", jobs)} (unspecified -- do not rely on it)");

        var stable = new PriorityQueue<string, (int Priority, int Sequence)>(
            Comparer<(int Priority, int Sequence)>.Create((x, y) =>
                x.Priority != y.Priority ? x.Priority.CompareTo(y.Priority) : x.Sequence.CompareTo(y.Sequence)));

        var seq = 0;
        foreach (var job in jobs)
        {
            stable.Enqueue(job, (1, seq++));
        }

        Console.WriteLine($"with a (priority, sequence) key: {DrainPq(stable)}");

        var mixed = new PriorityQueue<string, int>();
        mixed.Enqueue("low", 5);
        var swapped = mixed.EnqueueDequeue("high", 1);
        Console.WriteLine($"EnqueueDequeue(\"high\", 1) returned {swapped}; queue still holds {mixed.Peek()}");

        return Task.CompletedTask;

        static string DrainPq<TPriority>(PriorityQueue<string, TPriority> pq)
        {
            var items = new List<string>();
            while (pq.TryDequeue(out var item, out _))
            {
                items.Add(item);
            }

            return string.Join(", ", items);
        }
    }

    /// <summary>
    /// <see cref="SortedDictionary{TKey, TValue}"/> is a red-black tree: O(log n) insert that
    /// stays O(log n) no matter the order keys arrive in.
    /// <see cref="SortedList{TKey, TValue}"/> is two parallel arrays: O(n) insert in the worst
    /// case (it shifts to keep them sorted), less memory, and O(1) indexed access via
    /// <see cref="SortedList{TKey, TValue}.GetKeyAtIndex(int)"/>.
    /// </summary>
    /// <remarks>
    /// The allocation figures come from
    /// <see cref="GC.GetAllocatedBytesForCurrentThread"/> and vary a little between runs and
    /// runtimes; the invariant that holds is that the tree allocates more than the two arrays.
    /// </remarks>
    public static Task RunSortedDictionaryVsSortedListAsync()
    {
        var keys = Enumerable.Range(0, 5000).Select(i => (i * 2654435761L % 5000)).Select(k => (int)k).Distinct().ToArray();

        var beforeTree = GC.GetAllocatedBytesForCurrentThread();
        var tree = new SortedDictionary<int, int>();
        foreach (var k in keys)
        {
            tree[k] = k;
        }

        var treeBytes = GC.GetAllocatedBytesForCurrentThread() - beforeTree;

        var beforeArrays = GC.GetAllocatedBytesForCurrentThread();
        var arrays = new SortedList<int, int>();
        foreach (var k in keys)
        {
            arrays[k] = k;
        }

        var arrayBytes = GC.GetAllocatedBytesForCurrentThread() - beforeArrays;

        Console.WriteLine($"building from {keys.Length} shuffled keys:");
        Console.WriteLine($"  SortedDictionary allocated ~{treeBytes,8:N0} bytes");
        Console.WriteLine($"  SortedList       allocated ~{arrayBytes,8:N0} bytes");
        Console.WriteLine($"  tree allocates more than the arrays: {treeBytes > arrayBytes} [{(treeBytes > arrayBytes ? "holds" : "VIOLATED")}]");

        Console.WriteLine($"  SortedList.GetKeyAtIndex(0) = {arrays.GetKeyAtIndex(0)}, GetKeyAtIndex(^1) = {arrays.GetKeyAtIndex(arrays.Count - 1)}");
        Console.WriteLine($"  both enumerate in key order: {tree.Keys.SequenceEqual(arrays.Keys)} [{(tree.Keys.SequenceEqual(arrays.Keys) ? "agree" : "DIFFER")}]");

        return Task.CompletedTask;
    }
}

/* Expected output (RunQueueAndStackAsync)
queue Peek = build
queue drains FIFO: build -> test -> deploy
stack Peek = tokenize
stack drains LIFO: tokenize -> parse -> main
empty queue TryDequeue = False
empty queue Peek threw InvalidOperationException
*/

/* Expected output (RunLinkedListAsync)
after splicing around 'gamma': alpha, beta, gamma, gamma-and-a-half, delta
after Remove(node for 'gamma'): alpha, beta, gamma-and-a-half, delta
First = alpha, Last = delta
Find("missing") = null
*/

/* Expected output (RunPriorityQueueAsync) -- the equal-priority dequeue order is unspecified;
   this transcript is one runtime's heap behaviour, not a contract. The (priority, sequence)
   version is deterministic.
ten items, all priority 1, dequeue order: a, j, i, h, g, f, e, d, c, b
  came out in insertion order: False (unspecified -- do not rely on it)
with a (priority, sequence) key: a, b, c, d, e, f, g, h, i, j
EnqueueDequeue("high", 1) returned high; queue still holds low
*/

/* Expected output (RunSortedDictionaryVsSortedListAsync) -- byte counts are approximate and
   move a little between runtimes; the invariant is tree > arrays
building from 5000 shuffled keys:
  SortedDictionary allocated ~ 240,112 bytes
  SortedList       allocated ~ 131,680 bytes
  tree allocates more than the arrays: True [holds]
  SortedList.GetKeyAtIndex(0) = 0, GetKeyAtIndex(^1) = 4999
  both enumerate in key order: True [agree]
*/
