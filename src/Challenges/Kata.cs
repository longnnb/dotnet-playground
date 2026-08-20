using System.Text;

namespace Challenges;

/// <summary>
/// A handful of Codewars-style katas, each with a primary implementation plus one or more
/// alternatives that used to sit commented out beside it. Every alternative is now a real,
/// dispatched method whose result is compared against the primary implementation's, so
/// agreement (or divergence) is something the demo actually proves rather than something the
/// reader has to take on faith.
/// </summary>
public static class Kata
{
    /// <summary>
    /// Returns the divisors of <paramref name="n"/> strictly between 1 and <paramref name="n"/>,
    /// or <see langword="null"/> if <paramref name="n"/> is prime (or too small to have any).
    /// Checks every candidate from 2 to n-2.
    /// </summary>
    public static int[]? Divisors(int n)
    {
        if (n <= 3)
        {
            return null;
        }

        var result = Enumerable.Range(2, n - 3).Where(d => n % d == 0).ToArray();
        return result.Length == 0 ? null : result;
    }

    /// <summary>
    /// Same result as <see cref="Divisors"/>, but only checks candidates up to
    /// <c>sqrt(n)</c> and derives each candidate's pair (<c>x</c> and <c>n / x</c>) instead of
    /// scanning the full range. This resolves the "check the algorithm" question the original
    /// comment left open: the candidate range does need to go one past <c>(int)Math.Sqrt(n)</c>
    /// to avoid missing a divisor exactly at the square root.
    /// </summary>
    public static int[]? DivisorsBySqrt(int n)
    {
        if (n <= 3)
        {
            return null;
        }

        var divisors = Enumerable.Range(2, (int)Math.Sqrt(n))
            .Where(x => n % x == 0 && x < n)
            .SelectMany(x => new[] { x, n / x })
            .OrderBy(x => x)
            .Distinct()
            .ToArray();

        return divisors.Length == 0 ? null : divisors;
    }

    /// <summary>Counts how many characters appear more than once in <paramref name="str"/> (case-insensitive).</summary>
    public static int DuplicateCount(string str) =>
        str.ToLowerInvariant().GroupBy(c => c).Count(g => g.Count() > 1);

    /// <summary>The same count, as an explicit loop instead of a LINQ pipeline.</summary>
    public static int DuplicateCountManual(string str)
    {
        var lower = str.ToLowerInvariant();
        var result = 0;

        foreach (var c in lower.Distinct())
        {
            if (lower.Count(x => x == c) > 1)
            {
                result++;
            }
        }

        return result;
    }

    /// <summary>Adds two integers and returns the sum in binary.</summary>
    public static string AddBinary(int a, int b) => Convert.ToString(a + b, 2);

    /// <summary>
    /// The same sum-to-binary conversion built by hand with a <see cref="StringBuilder"/>.
    /// Unlike the naive version this once was, this handles a zero sum correctly -- a
    /// <c>while (c &gt; 0)</c> loop never executes for <c>c == 0</c>, so without the explicit
    /// check it would have returned an empty string instead of <c>"0"</c>.
    /// </summary>
    public static string AddBinaryManual(int a, int b)
    {
        var c = a + b;

        if (c == 0)
        {
            return "0";
        }

        var result = new StringBuilder();

        while (c > 0)
        {
            result.Insert(0, c % 2);
            c /= 2;
        }

        return result.ToString();
    }

    /// <summary>Counts the number of set bits (1s) in the binary representation of <paramref name="n"/>.</summary>
    public static int CountBits(int n) => Convert.ToString(n, 2).Count(c => c == '1');

    /// <summary>
    /// Tests whether <paramref name="n"/> is a perfect square by squaring the (possibly
    /// truncated) integer square root back and comparing against the exact original value.
    /// This self-corrects for floating-point rounding in <see cref="Math.Sqrt(double)"/> --
    /// see <see cref="IsSquareByModulo"/> for what happens without that correction.
    /// </summary>
    public static bool IsSquare(long n)
    {
        var r = (long)Math.Sqrt(n);
        return r * r == n;
    }

    /// <summary>
    /// Tests via <c>sqrt(n) % 1 == 0</c>: if the square root has no fractional part, it's a
    /// perfect square. Trusts <see cref="Math.Sqrt(double)"/>'s result directly, with no
    /// integer round-trip to catch a rounding error.
    /// </summary>
    public static bool IsSquareByModulo(long n) => Math.Sqrt(n) % 1 == 0;

    /// <summary>Tests via comparing <c>sqrt(n)</c> against its own rounded value -- same trust-the-float exposure as <see cref="IsSquareByModulo"/>.</summary>
    public static bool IsSquareByRounding(long n) => Math.Sqrt(n) == Math.Round(Math.Sqrt(n));

    /// <summary>
    /// Returns the characters that appear in either string, deduplicated and sorted, via
    /// <see cref="Enumerable.Union{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>
    /// (which deduplicates as part of combining the two sequences).
    /// </summary>
    public static string LongestByUnion(string s1, string s2) => string.Concat(s2.Union(s1).OrderBy(x => x));

    /// <summary>The same result via <see cref="Enumerable.Concat{TSource}"/> on the two character sequences, then a separate <c>Distinct</c>.</summary>
    public static string LongestByLinqConcat(string s1, string s2) =>
        new string(s1.Concat(s2).Distinct().OrderBy(c => c).ToArray());

    /// <summary>The same result via plain string concatenation (<c>s1 + s2</c>) before deduplicating.</summary>
    public static string LongestByStringConcat(string s1, string s2) => string.Concat((s1 + s2).Distinct().OrderBy(c => c));

    /// <summary>
    /// Finds the one missing letter in an otherwise-contiguous run, e.g. <c>['a','b','c','e']</c>
    /// is missing <c>'d'</c>. Uses <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>,
    /// which throws if nothing is missing -- the original used <c>FirstOrDefault</c>, which
    /// would have silently returned <c>'\0'</c> for that case instead of signaling a
    /// precondition violation.
    /// </summary>
    public static char FindMissingLetter(char[] array) =>
        (char)Enumerable.Range(array[0], array[^1] - array[0] + 1).First(x => !array.Contains((char)x));

    /// <summary>The same search via <see cref="Enumerable.Except{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/> instead of a linear scan with a per-element <c>Contains</c> check.</summary>
    public static char FindMissingLetterWithExcept(char[] array) =>
        Enumerable.Range(array[0], array.Length + 1).Select(a => (char)a).Except(array).Single();

    /// <summary>Runs <see cref="Divisors"/> and <see cref="DivisorsBySqrt"/> across n = 4..200 and reports any disagreement.</summary>
    public static Task RunDivisorsComparisonAsync()
    {
        var mismatches = new List<int>();

        for (var n = 4; n <= 200; n++)
        {
            var bruteForce = Divisors(n);
            var bySqrt = DivisorsBySqrt(n);

            if (!SequencesEqual(bruteForce, bySqrt))
            {
                mismatches.Add(n);
            }
        }

        Console.WriteLine(mismatches.Count == 0
            ? "Divisors: brute-force and sqrt-bounded variants agree for every n in [4, 200] [agree]"
            : $"Divisors: variants DIFFER for n = {string.Join(", ", mismatches)} [DIFFER]");

        Console.WriteLine($"Divisors(12) = [{string.Join(", ", Divisors(12) ?? [])}]");
        Console.WriteLine($"Divisors(13) = {(Divisors(13) is { } d ? $"[{string.Join(", ", d)}]" : "null (13 is prime)")}");

        return Task.CompletedTask;
    }

    private static bool SequencesEqual(int[]? a, int[]? b)
    {
        if (a is null || b is null)
        {
            return a is null && b is null;
        }

        return a.SequenceEqual(b);
    }

    /// <summary>Runs <see cref="DuplicateCount"/> and <see cref="DuplicateCountManual"/> over a handful of strings and compares.</summary>
    public static Task RunDuplicateCountComparisonAsync()
    {
        string[] examples = ["abcde", "aabbcde", "aabBcde", "indivisibility", ""];

        foreach (var s in examples)
        {
            var primary = DuplicateCount(s);
            var manual = DuplicateCountManual(s);
            Console.WriteLine($"DuplicateCount(\"{s}\"): primary={primary}  manual={manual}  [{(primary == manual ? "agree" : "DIFFER")}]");
        }

        return Task.CompletedTask;
    }

    /// <summary>Runs <see cref="AddBinary"/> and <see cref="AddBinaryManual"/> over several sums, including zero, and compares.</summary>
    public static Task RunAddBinaryComparisonAsync()
    {
        (int A, int B)[] cases = [(1, 1), (5, 9), (0, 0), (127, 1)];

        foreach (var (a, b) in cases)
        {
            var primary = AddBinary(a, b);
            var manual = AddBinaryManual(a, b);
            Console.WriteLine($"AddBinary({a}, {b}): primary={primary}  manual={manual}  [{(primary == manual ? "agree" : "DIFFER")}]");
        }

        return Task.CompletedTask;
    }

    /// <summary>Runs <see cref="CountBits"/> over a few values. No alternative implementation -- there's nothing to compare against.</summary>
    public static Task RunCountBitsAsync()
    {
        foreach (var n in new[] { 0, 1, 4, 7, 255 })
        {
            Console.WriteLine($"CountBits({n}) = {CountBits(n)}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Runs all three <c>IsSquare</c> variants across n = 1..10000 (where they agree), then
    /// against one large value chosen to expose the floating-point precision trap: past
    /// 2^53, a <see langword="long"/> can't round-trip through <see cref="double"/> exactly,
    /// so <see cref="Math.Sqrt(double)"/> can report a near-miss as an exact integer root.
    /// </summary>
    public static Task RunIsSquareComparisonAsync()
    {
        var mismatches = new List<long>();

        for (long n = 1; n <= 10_000; n++)
        {
            if (IsSquare(n) != IsSquareByModulo(n) || IsSquare(n) != IsSquareByRounding(n))
            {
                mismatches.Add(n);
            }
        }

        Console.WriteLine(mismatches.Count == 0
            ? "IsSquare: all three variants agree for every n in [1, 10000] [agree]"
            : $"IsSquare: variants DIFFER for n = {string.Join(", ", mismatches)} [DIFFER]");

        const long largeNonSquare = 9_999_999_999_999_999; // 10^16 - 1, one less than 100,000,000^2
        var byPrimary = IsSquare(largeNonSquare);
        var byModulo = IsSquareByModulo(largeNonSquare);
        var byRounding = IsSquareByRounding(largeNonSquare);
        var largeAgree = byPrimary == byModulo && byModulo == byRounding;

        Console.WriteLine(
            $"IsSquare({largeNonSquare}): primary={byPrimary}  byModulo={byModulo}  " +
            $"byRounding={byRounding}  [{(largeAgree ? "agree" : "DIFFER")}]");

        return Task.CompletedTask;
    }

    /// <summary>Runs all three <c>Longest</c> variants over a couple of examples and compares.</summary>
    public static Task RunLongestComparisonAsync()
    {
        (string S1, string S2)[] cases =
        [
            ("aretheyhere", "yestheyarehere"),
            ("loopingisfunbutdangerous", "lessdangerousthancoding"),
        ];

        foreach (var (s1, s2) in cases)
        {
            var byUnion = LongestByUnion(s1, s2);
            var byLinqConcat = LongestByLinqConcat(s1, s2);
            var byStringConcat = LongestByStringConcat(s1, s2);
            var agree = byUnion == byLinqConcat && byLinqConcat == byStringConcat;

            Console.WriteLine(
                $"Longest(\"{s1}\", \"{s2}\"): union=\"{byUnion}\"  linqConcat=\"{byLinqConcat}\"  " +
                $"stringConcat=\"{byStringConcat}\"  [{(agree ? "agree" : "DIFFER")}]");
        }

        return Task.CompletedTask;
    }

    /// <summary>Runs both <c>FindMissingLetter</c> variants over an example and compares.</summary>
    public static Task RunFindMissingLetterComparisonAsync()
    {
        char[] example = ['a', 'b', 'c', 'd', 'f'];
        var primary = FindMissingLetter(example);
        var viaExcept = FindMissingLetterWithExcept(example);

        Console.WriteLine(
            $"FindMissingLetter(['a','b','c','d','f']): primary='{primary}'  viaExcept='{viaExcept}'  " +
            $"[{(primary == viaExcept ? "agree" : "DIFFER")}]");

        return Task.CompletedTask;
    }
}
