using System.Globalization;

namespace Challenges;
public static class Jaden
{
    public static string ToJadenCase(this string phrase)
    {
        //my solution
        //return string.Join(' ', phrase.Split().Select(word => Char.ToUpper(word[0]) + word.Substring(1)));

        //Globalization
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(phrase);
    }
}