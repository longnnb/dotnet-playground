using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace CollectionTypes;

/// <summary>
/// Demonstrates the read-only and immutable collection families:
/// <see cref="ReadOnlyCollection{T}"/> (a <b>view</b>, not a copy), the
/// <see cref="System.Collections.Immutable"/> persistent collections (every mutation returns a
/// new instance), and the <see cref="System.Collections.Frozen"/> collections (build once,
/// slow; read many, fast).
/// </summary>
public static class ImmutableAndFrozenDemo
{
    /// <summary>
    /// <see cref="List{T}.AsReadOnly"/> wraps the same backing list rather than copying it.
    /// Callers cannot mutate through the wrapper, but anyone still holding the original list
    /// can -- and the "read-only" view shows the change.
    /// </summary>
    /// <remarks>
    /// To hand out a snapshot that genuinely cannot change under the caller, copy into an
    /// <see cref="ImmutableArray{T}"/> or <c>[.. source]</c> before exposing it.
    /// </remarks>
    public static Task RunReadOnlyViewAsync()
    {
        var backing = new List<string> { "a", "b", "c" };
        ReadOnlyCollection<string> view = backing.AsReadOnly();

        Console.WriteLine($"view starts as: [{string.Join(", ", view)}]");

        backing.Add("d");
        backing[0] = "A";
        Console.WriteLine($"after mutating the backing list, the view is: [{string.Join(", ", view)}]");

        var snapshot = backing.ToImmutableArray();
        backing.Add("e");
        Console.WriteLine($"an ImmutableArray snapshot taken earlier stays: [{string.Join(", ", snapshot)}]");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Immutable collections are persistent: <c>Add</c> / <c>SetItem</c> / <c>Remove</c> return
    /// a new collection and leave the receiver untouched. <c>CreateBuilder</c> gives you a
    /// mutable staging area so bulk construction is not O(n) allocations. An
    /// <c>ImmutableArray&lt;T&gt;</c> left at its <see langword="default"/> value is not empty
    /// but <b>uninitialised</b>, and touching it throws.
    /// </summary>
    public static Task RunImmutableAsync()
    {
        var original = ImmutableList.Create(1, 2, 3);
        var added = original.Add(4);
        Console.WriteLine($"original after .Add(4): [{string.Join(", ", original)}] (unchanged)");
        Console.WriteLine($"the value .Add(4) returned: [{string.Join(", ", added)}]");

        var builder = ImmutableArray.CreateBuilder<int>();
        for (var i = 0; i < 5; i++)
        {
            builder.Add(i * i);
        }

        var built = builder.ToImmutable();
        Console.WriteLine($"built via CreateBuilder: [{string.Join(", ", built)}]");

        ImmutableArray<int> uninitialised = default;
        Console.WriteLine($"default(ImmutableArray<int>).IsDefault = {uninitialised.IsDefault}, .IsDefaultOrEmpty = {uninitialised.IsDefaultOrEmpty}");
        try
        {
            _ = uninitialised.Length;
        }
        catch (NullReferenceException)
        {
            Console.WriteLine("reading .Length on a default ImmutableArray<int> threw NullReferenceException");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="FrozenDictionary{TKey, TValue}"/> and <see cref="FrozenSet{T}"/> spend extra
    /// time in <c>ToFrozen...</c> analysing the keys to pick a fast read strategy. They are
    /// immutable afterwards. Use them for lookup tables built once at startup and then read
    /// heavily; do not use them anywhere you would rebuild them often.
    /// </summary>
    public static Task RunFrozenAsync()
    {
        string[] keys = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];

        var frozen = keys.ToFrozenDictionary(k => k, k => k.Length, StringComparer.OrdinalIgnoreCase);
        Console.WriteLine($"FrozenDictionary built from {keys.Length} keys, Count = {frozen.Count}");
        Console.WriteLine($"  frozen[\"post\"] = {frozen["post"]} (comparer is case-insensitive)");
        Console.WriteLine($"  frozen.ContainsKey(\"TRACE\") = {frozen.ContainsKey("TRACE")}");

        var frozenSet = keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        Console.WriteLine($"  frozenSet.Contains(\"delete\") = {frozenSet.Contains("delete")}");

        try
        {
            ((IDictionary<string, int>)frozen).Add("TRACE", 5);
        }
        catch (NotSupportedException)
        {
            Console.WriteLine("  mutating the frozen dictionary threw NotSupportedException");
        }

        return Task.CompletedTask;
    }
}

/* Expected output (RunReadOnlyViewAsync)
view starts as: [a, b, c]
after mutating the backing list, the view is: [A, b, c, d]
an ImmutableArray snapshot taken earlier stays: [A, b, c, d]
*/

/* Expected output (RunImmutableAsync)
original after .Add(4): [1, 2, 3] (unchanged)
the value .Add(4) returned: [1, 2, 3, 4]
built via CreateBuilder: [0, 1, 4, 9, 16]
default(ImmutableArray<int>).IsDefault = True, .IsDefaultOrEmpty = True
reading .Length on a default ImmutableArray<int> threw NullReferenceException
*/

/* Expected output (RunFrozenAsync)
FrozenDictionary built from 7 keys, Count = 7
  frozen["post"] = 4 (comparer is case-insensitive)
  frozen.ContainsKey("TRACE") = False
  frozenSet.Contains("delete") = True
  mutating the frozen dictionary threw NotSupportedException
*/
