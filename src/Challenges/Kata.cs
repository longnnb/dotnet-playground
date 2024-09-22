namespace Challenges;

public class Kata
{
    public static int[] Divisors(int n)
    {
        //prime
        if (n <= 3) return null;
        var result = Enumerable.Range(2, n - 3).Where(d => n % d == 0).ToArray();
        return result.Length == 0 ? null : result;

        // check the alogorithm
        //var div = Enumerable.Range(2, (int)Math.Sqrt(n))
        //    .Where(x => n % x == 0 && x < n)
        //    .SelectMany(x => new[] { x, n / x })
        //    .OrderBy(x => x)
        //    .Distinct().ToArray();

        //return div.Any() ? div : null;
    }

    public static int DuplicateCount(string str)
    {
        // my solution
        //var result = 0;
        //var lowerStr = str.ToLower();
        //foreach (var c in lowerStr.Distinct())
        //{
        //    result += lowerStr.Count(x => x == c) > 1 ? 1 : 0;
        //}
        //return result;

        //concise
        return str.ToLower().GroupBy(c => c).Where(g => g.Count() > 1).Count();
        //or
        //return str.ToLower().GroupBy(c => c).Count(c => c.Count() > 1);
    }

    public static string AddBinary(int a, int b)
    {
        //my solution
        //var c = a + b;
        //var result = new StringBuilder();
        //while (c > 0)
        //{
        //    result.Insert(0, c % 2);
        //    c /= 2;
        //}
        //return result.ToString();

        //consice
        return Convert.ToString(a + b, 2);
    }

    public static int CountBits(int n)
    {
        return Convert.ToString(n, 2).Count(c => c == '1');
    }

    public static bool IsSquare(int n)
    {
        var r = (int)Math.Sqrt(n);
        if (r * r == n)
            return true;
        return false;

        //consice
        //return Math.Sqrt(n) % 1 == 0;
        //return Math.Sqrt(n) == (int) Math.Sqrt(n);
    }

    public static string Longest(string s1, string s2)
    {
        //return new String(s1.Concat(s2).Distinct().OrderBy(c => c).ToArray());
        //return string.Concat((s1 + s2).Distinct().OrderBy(c => c));
        return string.Concat(s2.Union(s1).OrderBy(x => x));
    }

    public static char FindMissingLetter(char[] array)
    {
        return (char)Enumerable.Range(array.First(), array.Last() - array.First() + 1)
            .FirstOrDefault(x => !array.Contains((char)x));
        //return Enumerable.Range(array[0], array.Length + 1).Select(a => (char)a).Except(array).Single();
    }
}