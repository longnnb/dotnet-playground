namespace CollectionTypes;

/// <summary>
/// Demonstrates how <see cref="HashSet{T}"/> and <see cref="Dictionary{TKey, TValue}"/> decide
/// whether two elements are "the same": the reference-equality default on a plain
/// <see langword="class"/>, the value equality a <see langword="record"/> generates, a custom
/// <see cref="IEqualityComparer{T}"/>, and the entry-you-can-no-longer-find bug you get by
/// mutating a field that feeds <see cref="object.GetHashCode"/> after insertion.
/// </summary>
public static class EqualityComparerDemo
{
    /// <summary>
    /// A <see langword="class"/> without an <c>Equals</c> override uses reference equality, so
    /// two "equal-looking" instances are distinct set members. A <see langword="record"/> and a
    /// <see langword="record struct"/> both get compiler-generated structural
    /// <c>Equals</c>/<c>GetHashCode</c>, so they deduplicate.
    /// </summary>
    public static Task RunDefaultAsync()
    {
        var asClass = new HashSet<PointClass> { new(1, 2), new(1, 2), new(3, 4) };
        var asRecord = new HashSet<PointRecord> { new(1, 2), new(1, 2), new(3, 4) };
        var asRecordStruct = new HashSet<PointRecordStruct> { new(1, 2), new(1, 2), new(3, 4) };

        Console.WriteLine($"HashSet<class>         after adding (1,2),(1,2),(3,4): Count = {asClass.Count} (reference equality)");
        Console.WriteLine($"HashSet<record>        after the same adds:            Count = {asRecord.Count} (value equality)");
        Console.WriteLine($"HashSet<record struct> after the same adds:            Count = {asRecordStruct.Count} (value equality)");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Passing an <see cref="IEqualityComparer{T}"/> (here
    /// <see cref="StringComparer.OrdinalIgnoreCase"/>, plus a hand-written one) changes the
    /// notion of key identity without touching the key type.
    /// </summary>
    public static Task RunCustomComparerAsync()
    {
        var caseInsensitive = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Hello"] = 1,
        };
        caseInsensitive["HELLO"] += 10;
        Console.WriteLine($"OrdinalIgnoreCase dictionary: \"hello\" -> {caseInsensitive["hello"]}, Count = {caseInsensitive.Count}");

        var byLastName = new HashSet<Person>(new LastNameComparer())
        {
            new("Ada", "Lovelace"),
            new("Augusta", "Lovelace"),
            new("Alan", "Turing"),
        };
        Console.WriteLine($"HashSet<Person> keyed on last name only: Count = {byLastName.Count} ({string.Join(", ", byLastName.Select(p => p.Last))})");

        return Task.CompletedTask;
    }

    /// <summary>
    /// A hash-based collection bins each element by its hash code <b>at insertion time</b>.
    /// Mutating a field that <c>GetHashCode</c> depends on afterwards leaves the element in the
    /// wrong bin: it is still enumerable and still <c>==</c> to an equal probe, but
    /// <c>Contains</c> hashes the probe to a different bin and misses it.
    /// </summary>
    public static Task RunMutableKeyAsync()
    {
        var key = new MutableKey(1);
        var set = new HashSet<MutableKey> { key };

        Console.WriteLine($"just after Add: set.Contains(key) = {set.Contains(key)}");

        key.Id = 999;

        Console.WriteLine($"after key.Id = 999:");
        Console.WriteLine($"  set.Contains(key)                 = {set.Contains(key)} (hashes to the wrong bucket)");
        Console.WriteLine($"  set.Any(x => ReferenceEquals(x, key)) = {set.Any(x => ReferenceEquals(x, key))} (still in there)");
        Console.WriteLine($"  set.Count                        = {set.Count}");

        return Task.CompletedTask;
    }
}

/// <summary>A last-name-only equality comparer for <see cref="Person"/>.</summary>
file sealed class LastNameComparer : IEqualityComparer<Person>
{
    public bool Equals(Person? x, Person? y) => string.Equals(x?.Last, y?.Last, StringComparison.Ordinal);

    public int GetHashCode(Person obj) => obj.Last.GetHashCode(StringComparison.Ordinal);
}

/// <summary>A person with a first and last name.</summary>
/// <param name="First">Given name.</param>
/// <param name="Last">Family name.</param>
file sealed record Person(string First, string Last);

/// <summary>A reference type with no equality override -- compared by reference.</summary>
file sealed class PointClass(int x, int y)
{
    /// <summary>X coordinate.</summary>
    public int X { get; } = x;

    /// <summary>Y coordinate.</summary>
    public int Y { get; } = y;
}

/// <summary>A reference type with compiler-generated value equality.</summary>
/// <param name="X">X coordinate.</param>
/// <param name="Y">Y coordinate.</param>
file sealed record PointRecord(int X, int Y);

/// <summary>A value type with compiler-generated value equality.</summary>
/// <param name="X">X coordinate.</param>
/// <param name="Y">Y coordinate.</param>
file readonly record struct PointRecordStruct(int X, int Y);

/// <summary>A key whose hash code changes when <see cref="Id"/> is reassigned.</summary>
file sealed class MutableKey(int id)
{
    /// <summary>The identity field -- also the whole hash code, so mutating it re-bins the object.</summary>
    public int Id { get; set; } = id;

    public override bool Equals(object? obj) => obj is MutableKey other && other.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();
}

/* Expected output (RunDefaultAsync)
HashSet<class>         after adding (1,2),(1,2),(3,4): Count = 3 (reference equality)
HashSet<record>        after the same adds:            Count = 2 (value equality)
HashSet<record struct> after the same adds:            Count = 2 (value equality)
*/

/* Expected output (RunCustomComparerAsync)
OrdinalIgnoreCase dictionary: "hello" -> 11, Count = 1
HashSet<Person> keyed on last name only: Count = 2 (Lovelace, Turing)
*/

/* Expected output (RunMutableKeyAsync)
just after Add: set.Contains(key) = True
after key.Id = 999:
  set.Contains(key)                 = False (hashes to the wrong bucket)
  set.Any(x => ReferenceEquals(x, key)) = True (still in there)
  set.Count                        = 1
*/
