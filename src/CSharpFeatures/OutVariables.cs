using static System.Console;

namespace CSharpFeatures;
public class OutVariables
{
    public void TryParseInt()
    {
        string input = "123";
        // try parse to int and assign to result
        if (int.TryParse(input, out int result))
        {
            WriteLine(result);
        }

        // what if the parse fails?
        int.TryParse("abc", out var i);
        WriteLine($"i = {i}"); // default value
    }

    public void TryParseDateTime()
    {
        DateTime dt; // struct
        if (DateTime.TryParse("01/01/2017", out dt))
        {
            WriteLine($"Old-fashioned parse: {dt}");
        }

        // variable declaration is an expression, not a statement
        if (DateTime.TryParse("02/02/2016", out /*DateTime*/ var dt2))
        {
            WriteLine($"New parse: {dt2}");
        }

        // the scope of dt2 extends outside the if block
        WriteLine($"I can use dt2 here: {dt2}");
    }
}
