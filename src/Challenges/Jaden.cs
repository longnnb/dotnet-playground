using System.Globalization;

namespace Challenges;

/// <summary>
/// The Codewars "Jaden Casing Strings" kata (capitalize every word), used as the vehicle for
/// two different extension mechanisms: a C# 14 <c>extension(string phrase) {{ ... }}</c> block
/// and the classic pre-C#14 <c>this string phrase</c> extension method.
/// </summary>
public static class JadenExtensions
{
    extension(string phrase)
    {
        /// <summary>
        /// Title-cases <paramref name="phrase"/> via <see cref="TextInfo.ToTitleCase"/>. Per its
        /// documented behavior, a word that is already entirely uppercase is presumed to be an
        /// acronym and is left untouched -- see <see cref="ToJadenCaseManual"/> for where that
        /// causes the two to genuinely disagree.
        /// </summary>
        public string ToJadenCase() => CultureInfo.GetCultureInfo("en-US").TextInfo.ToTitleCase(phrase);
    }

    /// <summary>
    /// The naive hand-rolled version most people reach for first: capitalize each word's first
    /// letter and force everything else to lowercase. This is <em>not</em> the same operation as
    /// <see cref="ToJadenCase"/> -- it has no concept of an acronym, so an all-caps word like
    /// "USA" becomes "Usa" instead of staying "USA". (A version that merely capitalized the
    /// first letter and left the rest alone, which is what a first read of this kata tends to
    /// produce, turns out to never disagree with <c>ToTitleCase</c> at all: neither one touches
    /// characters after the first, so there's nothing to compare. Forcing the remainder to
    /// lowercase is what actually exercises the acronym rule.)
    /// </summary>
    public static string ToJadenCaseManual(this string phrase) =>
        string.Join(' ', phrase.Split(' ').Select(word =>
            word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));

    /// <summary>
    /// Runs both casing strategies over a mixed-case phrase (where they agree) and an all-caps
    /// phrase (where they don't, because <see cref="TextInfo.ToTitleCase"/> refuses to touch it).
    /// </summary>
    public static Task RunJadenCaseComparisonAsync()
    {
        string[] phrases = ["How can mirrors be real", "HOW CAN MIRRORS BE REAL", "the quick brown fox"];

        foreach (var phrase in phrases)
        {
            var viaTitleCase = phrase.ToJadenCase();
            var manual = phrase.ToJadenCaseManual();
            Console.WriteLine($"\"{phrase}\":");
            Console.WriteLine($"  ToTitleCase: \"{viaTitleCase}\"");
            Console.WriteLine($"  manual:      \"{manual}\"");
            Console.WriteLine($"  [{(viaTitleCase == manual ? "agree" : "DIFFER")}]");
        }

        return Task.CompletedTask;
    }
}
