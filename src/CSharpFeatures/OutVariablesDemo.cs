using System.Globalization;

namespace CSharpFeatures;

/// <summary>
/// Demonstrates C# 7 "out variables": declaring the <c>out</c> argument inline at the call site
/// (<c>out var x</c>), the fact that a variable declaration is an expression rather than a
/// statement, and where its scope actually ends.
/// </summary>
public static class OutVariablesDemo
{
    /// <summary>
    /// Contrasts the pre-C#7 two-statement form with the inline <c>out var</c> form, and shows
    /// what <see cref="int.TryParse(string?, out int)"/> leaves in the out parameter when
    /// parsing fails: the type's default, not an exception and not an unassigned variable.
    /// </summary>
    public static Task RunAsync()
    {
        // Old-fashioned: declare, then pass by ref.
        var parsedOldStyle = int.TryParse("123", out var oldStyleResult);
        Console.WriteLine($"Old-fashioned parse succeeded={parsedOldStyle}, result={oldStyleResult}");

        // C# 7: the out argument is declared inline.
        if (int.TryParse("123", out var result))
        {
            Console.WriteLine($"Inline out var: {result}");
        }

        // TryParse never throws: on failure it sets the out parameter to default(T) -- 0 for
        // int -- and returns false. It does not leave the variable unassigned. The bool is
        // discarded on purpose here -- that's the point being demonstrated.
        _ = int.TryParse("abc", out var failedParse);
        Console.WriteLine($"Failed parse leaves the default value: {failedParse}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Shows that a variable declared in an <c>out var</c> inside an <c>if</c> condition is
    /// scoped to the rest of the enclosing block, not just the <c>if</c> body -- the classic
    /// surprise the first time you meet the feature.
    /// </summary>
    public static Task RunScopeAsync()
    {
        if (int.TryParse("42", out var value))
        {
            Console.WriteLine($"Inside the if: {value}");
        }

        // `value` is still in scope here, even though we're past the `if` block that declared it.
        Console.WriteLine($"Still in scope after the if: {value}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// The discard <c>out _</c> for when only the boolean result matters, and the same
    /// <c>out var</c> syntax powering <see cref="Dictionary{TKey,TValue}.TryGetValue"/>.
    /// </summary>
    public static Task RunDiscardAndDictionaryAsync()
    {
        var isValid = int.TryParse("999", out _);
        Console.WriteLine($"Discarded the value, kept only the bool: {isValid}");

        var ages = new Dictionary<string, int> { ["Ada"] = 36, ["Alan"] = 41 };

        if (ages.TryGetValue("Ada", out var age))
        {
            Console.WriteLine($"Ada is {age}");
        }

        if (!ages.TryGetValue("Grace", out var missingAge))
        {
            Console.WriteLine($"Grace isn't in the dictionary; out var still gave us the default: {missingAge}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="DateTime.TryParse(string?, out DateTime)"/> without an explicit
    /// <see cref="CultureInfo"/> parses using <see cref="CultureInfo.CurrentCulture"/>, so the
    /// exact same input string can parse to two different dates depending on which machine runs
    /// it -- day/month on an en-GB box, month/day on en-US. Passing
    /// <see cref="CultureInfo.InvariantCulture"/> explicitly makes the result reproducible.
    /// </summary>
    public static Task RunCulturePitfallAsync()
    {
        const string input = "01/02/2017";

        var enUs = DateTime.Parse(input, CultureInfo.GetCultureInfo("en-US"));
        var enGb = DateTime.Parse(input, CultureInfo.GetCultureInfo("en-GB"));
        var invariant = DateTime.Parse(input, CultureInfo.InvariantCulture);

        Console.WriteLine($"\"{input}\" under en-US:              {enUs:yyyy-MM-dd} (month/day)");
        Console.WriteLine($"\"{input}\" under en-GB:              {enGb:yyyy-MM-dd} (day/month)");
        Console.WriteLine($"\"{input}\" under InvariantCulture:   {invariant:yyyy-MM-dd} (month/day -- but explicit and reproducible)");

        return Task.CompletedTask;
    }
}

/* Expected output (RunAsync)
Old-fashioned parse succeeded=True, result=123
Inline out var: 123
Failed parse leaves the default value: 0
*/

/* Expected output (RunScopeAsync)
Inside the if: 42
Still in scope after the if: 42
*/

/* Expected output (RunDiscardAndDictionaryAsync)
Discarded the value, kept only the bool: True
Ada is 36
Grace isn't in the dictionary; out var still gave us the default: 0
*/

/* Expected output (RunCulturePitfallAsync)
"01/02/2017" under en-US:              2017-01-02 (month/day)
"01/02/2017" under en-GB:              2017-02-01 (day/month)
"01/02/2017" under InvariantCulture:   2017-01-02 (month/day -- but explicit and reproducible)
*/
