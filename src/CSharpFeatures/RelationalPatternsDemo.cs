using System.Globalization;

namespace CSharpFeatures;

/// <summary>
/// Demonstrates C# 9 relational (<c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c>, <c>&gt;=</c>) and
/// logical (<c>and</c>, <c>or</c>, <c>not</c>) pattern combinators, which express range and
/// null checks directly in a pattern instead of a chain of <c>&amp;&amp;</c>/<c>||</c>.
/// </summary>
public static class RelationalPatternsDemo
{
    /// <summary>Runs the demo over a handful of representative values, including <see langword="null"/>.</summary>
    public static Task RunAsync()
    {
        foreach (var value in new int?[] { -5, 0, 7, 42, 150, null })
        {
            var label = value?.ToString(CultureInfo.InvariantCulture) ?? "null";
            Console.WriteLine($"{label,5} => {Classify(value)}");
        }

        return Task.CompletedTask;
    }

    private static string Classify(int? value) => value switch
    {
        null => "missing",
        < 0 => "negative",
        0 => "zero",
        > 0 and < 10 => "single digit",
        >= 10 and <= 99 => "double digit",
        _ => "large",
    };
}

/* Expected output
   -5 => negative
    0 => zero
    7 => single digit
   42 => double digit
  150 => large
 null => missing
*/
