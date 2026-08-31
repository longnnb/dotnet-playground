using System.Runtime.InteropServices;

namespace CollectionTypes;

/// <summary>
/// Demonstrates <see cref="Dictionary{TKey, TValue}"/>: the lookup APIs and how many times each
/// one hashes the key, why enumeration order is not insertion order and not guaranteed,
/// <see cref="CollectionsMarshal.GetValueRefOrAddDefault{TKey, TValue}(Dictionary{TKey, TValue}, TKey, out bool)"/>
/// for single-lookup update-in-place, and <c>GetAlternateLookup</c> for probing a
/// string-keyed dictionary with a <see cref="ReadOnlySpan{T}"/> and no allocation.
/// </summary>
public static class DictionaryDemo
{
    /// <summary>
    /// <c>ContainsKey</c> followed by the indexer hashes the key twice and walks the bucket
    /// twice; <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> does it once.
    /// A comparer that counts its <c>GetHashCode</c> calls makes that concrete.
    /// <c>Add</c> throws on a duplicate key, the indexer overwrites, and
    /// <see cref="Dictionary{TKey, TValue}.TryAdd(TKey, TValue)"/> reports failure without
    /// throwing.
    /// </summary>
    public static Task RunLookupAsync()
    {
        var counting = new HashCountingComparer();
        var inventory = new Dictionary<string, int>(counting)
        {
            ["apple"] = 3,
            ["pear"] = 0,
        };

        counting.Reset();
        var present = inventory.ContainsKey("apple");
        var value = inventory["apple"];
        Console.WriteLine($"ContainsKey(\"apple\")={present} then inventory[\"apple\"]={value}: {counting.HashCalls} hash calls");

        counting.Reset();
        inventory.TryGetValue("apple", out var count);
        Console.WriteLine($"TryGetValue(\"apple\", out {count}): {counting.HashCalls} hash call");

        Console.WriteLine($"GetValueOrDefault(\"pear\")   = {inventory.GetValueOrDefault("pear")}");
        Console.WriteLine($"GetValueOrDefault(\"quince\") = {inventory.GetValueOrDefault("quince")} (absent -> default(int))");
        Console.WriteLine($"GetValueOrDefault(\"quince\", -1) = {inventory.GetValueOrDefault("quince", -1)}");

        Console.WriteLine($"TryAdd(\"apple\", 9) = {inventory.TryAdd("apple", 9)} (key already present)");
        inventory["apple"] = 9;
        Console.WriteLine($"indexer overwrite: apple = {inventory["apple"]}");

        try
        {
            inventory.Add("apple", 1);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Add(\"apple\", 1) threw {ex.GetType().Name}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Enumeration order is an implementation detail. It usually looks like insertion order
    /// until you remove a key -- the freed slot is the first one a later insert reuses, so the
    /// new key appears where the old one was, not at the end.
    /// </summary>
    public static Task RunOrderingAsync()
    {
        var d = new Dictionary<string, int>();
        foreach (var (key, value) in new[] { ("one", 1), ("two", 2), ("three", 3), ("four", 4) })
        {
            d[key] = value;
        }

        Console.WriteLine($"after inserting one..four: {string.Join(", ", d.Keys)}");

        d.Remove("two");
        d["five"] = 5;
        Console.WriteLine($"after Remove(\"two\"), Add(\"five\"): {string.Join(", ", d.Keys)}");
        Console.WriteLine("\"five\" landed in \"two\"'s freed slot rather than at the end.");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Counting occurrences the obvious way does two dictionary operations per item
    /// (<c>TryGetValue</c> then indexer-set).
    /// <see cref="CollectionsMarshal.GetValueRefOrAddDefault{TKey, TValue}(Dictionary{TKey, TValue}, TKey, out bool)"/>
    /// returns a <c>ref</c> to the slot -- present or freshly zero-initialised -- so you hash
    /// once and mutate through the reference. Both produce the same histogram.
    /// </summary>
    public static Task RunRefAccessAsync()
    {
        const string text = "the quick brown fox the lazy dog the end";
        var words = text.Split(' ');

        var byTwoLookups = new Dictionary<string, int>();
        foreach (var word in words)
        {
            byTwoLookups.TryGetValue(word, out var current);
            byTwoLookups[word] = current + 1;
        }

        var byRef = new Dictionary<string, int>();
        foreach (var word in words)
        {
            ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(byRef, word, out _);
            slot++;
        }

        var agree = byTwoLookups.Count == byRef.Count && byTwoLookups.All(kv => byRef.GetValueOrDefault(kv.Key) == kv.Value);
        Console.WriteLine($"two-lookup histogram: {Format(byTwoLookups)}");
        Console.WriteLine($"ref-access histogram: {Format(byRef)}");
        Console.WriteLine($"histograms match [{(agree ? "agree" : "DIFFER")}]");

        return Task.CompletedTask;

        static string Format(Dictionary<string, int> d) =>
            string.Join(", ", d.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));
    }

    /// <summary>
    /// A <see cref="Dictionary{TKey, TValue}"/> whose comparer supports it (here
    /// <see cref="StringComparer.OrdinalIgnoreCase"/>) exposes <c>GetAlternateLookup</c>: you
    /// can query it with a <c>ReadOnlySpan&lt;char&gt;</c> -- a slice of a larger string, for
    /// instance -- without first materialising that slice as its own <see cref="string"/>.
    /// </summary>
    public static Task RunAlternateLookupAsync()
    {
        var codes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["US"] = "United States",
            ["GB"] = "United Kingdom",
            ["DE"] = "Germany",
        };

        var lookup = codes.GetAlternateLookup<ReadOnlySpan<char>>();

        const string csv = "US,GB,FR,DE";
        foreach (var range in csv.AsSpan().Split(','))
        {
            var token = csv.AsSpan(range);
            var known = lookup.TryGetValue(token, out var name);
            Console.WriteLine($"  {token} -> {(known ? name : "(unknown)")}");
        }

        Console.WriteLine($"lookup[\"gb\".AsSpan()] = {lookup["gb".AsSpan()]} (comparer is case-insensitive)");

        return Task.CompletedTask;
    }
}

/// <summary>
/// An ordinal string comparer that tallies how many times the dictionary asks it to hash a key.
/// </summary>
file sealed class HashCountingComparer : IEqualityComparer<string>
{
    /// <summary>Number of <see cref="GetHashCode(string)"/> calls since the last <see cref="Reset"/>.</summary>
    public int HashCalls { get; private set; }

    /// <summary>Zeroes <see cref="HashCalls"/>.</summary>
    public void Reset() => HashCalls = 0;

    /// <inheritdoc/>
    public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.Ordinal);

    /// <inheritdoc/>
    public int GetHashCode(string obj)
    {
        HashCalls++;
        return obj.GetHashCode(StringComparison.Ordinal);
    }
}

/* Expected output (RunLookupAsync)
ContainsKey("apple")=True then inventory["apple"]=3: 2 hash calls
TryGetValue("apple", out 3): 1 hash call
GetValueOrDefault("pear")   = 0
GetValueOrDefault("quince") = 0 (absent -> default(int))
GetValueOrDefault("quince", -1) = -1
TryAdd("apple", 9) = False (key already present)
indexer overwrite: apple = 9
Add("apple", 1) threw ArgumentException
*/

/* Expected output (RunOrderingAsync) -- the exact enumeration order is an implementation detail;
   the point is that "five" does not appear last
after inserting one..four: one, two, three, four
after Remove("two"), Add("five"): one, five, three, four
"five" landed in "two"'s freed slot rather than at the end.
*/

/* Expected output (RunRefAccessAsync)
two-lookup histogram: brown=1, dog=1, end=1, fox=1, lazy=1, quick=1, the=3
ref-access histogram: brown=1, dog=1, end=1, fox=1, lazy=1, quick=1, the=3
histograms match [agree]
*/

/* Expected output (RunAlternateLookupAsync)
  US -> United States
  GB -> United Kingdom
  FR -> (unknown)
  DE -> Germany
lookup["gb".AsSpan()] = United Kingdom (comparer is case-insensitive)
*/
