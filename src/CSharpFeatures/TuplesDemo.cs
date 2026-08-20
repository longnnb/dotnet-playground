namespace CSharpFeatures;

/// <summary>
/// Demonstrates C# tuples: named <see cref="ValueTuple"/> elements, deconstruction, structural
/// equality (and its absence on <see cref="System.Tuple"/>), tuples as dictionary keys, the
/// fact that element names are compile-time only, tuples in LINQ, and when to graduate a tuple
/// into a proper type.
/// </summary>
public static class TuplesDemo
{
    /// <summary>Named tuple elements, and deconstructing a tuple return value into two locals.</summary>
    public static Task RunBasicsAsync()
    {
        var (min, max) = MinMax([5, 1, 9, 3]);
        Console.WriteLine($"MinMax([5, 1, 9, 3]) => Min={min}, Max={max}");

        // The names are visible on the tuple itself too, not just after deconstructing it.
        var range = MinMax([5, 1, 9, 3]);
        Console.WriteLine($"Same call without deconstructing: range.Min={range.Min}, range.Max={range.Max}");

        return Task.CompletedTask;
    }

    private static (int Min, int Max) MinMax(ReadOnlySpan<int> values)
    {
        var min = values[0];
        var max = values[0];

        foreach (var value in values)
        {
            if (value < min)
            {
                min = value;
            }

            if (value > max)
            {
                max = value;
            }
        }

        return (min, max);
    }

    /// <summary>
    /// The same operation written two ways: with <c>out</c> parameters (see
    /// <see cref="OutVariablesDemo"/>) and with a tuple return. A tuple return composes with
    /// LINQ and expressions; an <c>out</c> parameter can't be used inline in either.
    /// </summary>
    public static Task RunTupleVsOutParametersAsync()
    {
        DivideWithOut(17, 5, out var quotient, out var remainder);
        Console.WriteLine($"With out params: 17 / 5 = {quotient} remainder {remainder}");

        var (tupleQuotient, tupleRemainder) = DivideWithTuple(17, 5);
        Console.WriteLine($"With a tuple:    17 / 5 = {tupleQuotient} remainder {tupleRemainder}");

        // The tuple form composes directly with LINQ; the out-param form would need a local
        // function wrapper to be used inline in a lambda at all.
        var remainders = new (int Dividend, int Divisor)[] { (17, 5), (20, 6), (9, 4) }
            .Select(pair => DivideWithTuple(pair.Dividend, pair.Divisor).Remainder);
        Console.WriteLine($"Remainders via LINQ: {string.Join(", ", remainders)}");

        return Task.CompletedTask;
    }

    private static void DivideWithOut(int dividend, int divisor, out int quotient, out int remainder)
    {
        quotient = dividend / divisor;
        remainder = dividend % divisor;
    }

    private static (int Quotient, int Remainder) DivideWithTuple(int dividend, int divisor) =>
        (dividend / divisor, dividend % divisor);

    /// <summary>A custom <c>Deconstruct</c> method makes any type deconstructable, not just tuples -- and discards work on either.</summary>
    public static Task RunDeconstructionAsync()
    {
        var point = new Point(3, 4);
        var (x, y) = point;
        Console.WriteLine($"Deconstructed point: x={x}, y={y}");

        var (_, yOnly) = point;
        Console.WriteLine($"Discarding x, keeping only y: {yOnly}");

        return Task.CompletedTask;
    }

    private readonly struct Point(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }
    }

    /// <summary>
    /// <see cref="ValueTuple"/> has structural equality -- two tuples with the same element
    /// values are equal regardless of their (compile-time-only) element names.
    /// <see cref="System.Tuple"/>, the older reference-typed alternative, is not: its <c>==</c>
    /// falls back to reference equality even though its <c>Equals</c> is structural.
    /// </summary>
    public static Task RunEqualityAsync()
    {
        (int Min, int Max) a = (1, 2);
        (int Low, int High) b = (1, 2);
        Console.WriteLine($"ValueTuple (1,2) == (1,2), different names: {a == b}");

        var refA = System.Tuple.Create(1, 2);
        var refB = System.Tuple.Create(1, 2);
        Console.WriteLine($"System.Tuple.Create(1,2) == System.Tuple.Create(1,2): {refA == refB} (reference equality)");
        Console.WriteLine($"...but .Equals compares values: {refA.Equals(refB)}");

        return Task.CompletedTask;
    }

    /// <summary>Tuples work as dictionary keys out of the box, using their structural <c>GetHashCode</c>.</summary>
    public static Task RunAsDictionaryKeysAsync()
    {
        var grid = new Dictionary<(int Row, int Col), string>
        {
            [(0, 0)] = "origin",
            [(1, 2)] = "somewhere",
        };

        Console.WriteLine($"grid[(0, 0)] = {grid[(0, 0)]}");
        Console.WriteLine($"grid.ContainsKey((1, 2)) = {grid.ContainsKey((1, 2))}");
        Console.WriteLine($"grid.ContainsKey((2, 1)) = {grid.ContainsKey((2, 1))}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Element names exist only at compile time -- they're compiler sugar over
    /// <see cref="ValueTuple{T1,T2}"/>'s <c>Item1</c>/<c>Item2</c> fields. <c>ToString()</c>
    /// never mentions them, and reflection reports the unnamed generic type.
    /// </summary>
    public static Task RunNameErasureAsync()
    {
        (int Min, int Max) range = (1, 2);
        Console.WriteLine($"ToString() ignores the names: {range}");
        Console.WriteLine($"Reflection sees the unnamed generic type: {range.GetType()}");

        return Task.CompletedTask;
    }

    /// <summary>Grouping into named tuples with LINQ, then pattern-matching on a tuple in a <c>switch</c> expression.</summary>
    public static Task RunInLinqAsync()
    {
        string[] words = ["apple", "avocado", "banana", "blueberry", "cherry"];

        var byFirstLetter = words
            .GroupBy(word => word[0])
            .Select(group => (Letter: group.Key, Count: group.Count()))
            .OrderBy(g => g.Letter);

        foreach (var (letter, count) in byFirstLetter)
        {
            Console.WriteLine($"{letter}: {count} word(s) -- {Classify((letter, count))}");
        }

        return Task.CompletedTask;
    }

    private static string Classify((char Letter, int Count) group) => group switch
    {
        (_, 1) => "unique starting letter",
        (_, > 1) => "shared starting letter",
        _ => "unexpected",
    };

    /// <summary>
    /// When a tuple's fields need a name that documents intent everywhere -- not just at one
    /// call site -- or need validation or behavior, graduate it to a <c>record struct</c>: the
    /// same value semantics and no heap allocation, but a real, self-describing type.
    /// </summary>
    public static Task RunTupleVsRecordAsync()
    {
        (double Value, string Unit) asTuple = (98.6, "F");
        Console.WriteLine($"As a tuple: {asTuple.Value} {asTuple.Unit}");

        var asRecord = new Measurement(98.6, "F");
        Console.WriteLine($"As a record struct: {asRecord} -- and it can validate: IsHuman()={asRecord.IsHuman()}");

        return Task.CompletedTask;
    }

    private readonly record struct Measurement(double Value, string Unit)
    {
        public bool IsHuman() => Unit == "F" && Value is > 90 and < 110;
    }
}

/* Expected output (RunBasicsAsync)
MinMax([5, 1, 9, 3]) => Min=1, Max=9
Same call without deconstructing: range.Min=1, range.Max=9
*/

/* Expected output (RunTupleVsOutParametersAsync)
With out params: 17 / 5 = 3 remainder 2
With a tuple:    17 / 5 = 3 remainder 2
Remainders via LINQ: 2, 2, 1
*/

/* Expected output (RunDeconstructionAsync)
Deconstructed point: x=3, y=4
Discarding x, keeping only y: 4
*/

/* Expected output (RunEqualityAsync)
ValueTuple (1,2) == (1,2), different names: True
System.Tuple.Create(1,2) == System.Tuple.Create(1,2): False (reference equality)
...but .Equals compares values: True
*/

/* Expected output (RunAsDictionaryKeysAsync)
grid[(0, 0)] = origin
grid.ContainsKey((1, 2)) = True
grid.ContainsKey((2, 1)) = False
*/

/* Expected output (RunNameErasureAsync)
ToString() ignores the names: (1, 2)
Reflection sees the unnamed generic type: System.ValueTuple`2[System.Int32,System.Int32]
*/

/* Expected output (RunInLinqAsync)
a: 2 word(s) -- shared starting letter
b: 2 word(s) -- shared starting letter
c: 1 word(s) -- unique starting letter
*/

/* Expected output (RunTupleVsRecordAsync)
As a tuple: 98.6 F
As a record struct: Measurement { Value = 98.6, Unit = F } -- and it can validate: IsHuman()=True
*/
