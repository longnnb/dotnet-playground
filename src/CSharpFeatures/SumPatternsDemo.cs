namespace CSharpFeatures;

/// <summary>
/// Demonstrates three equivalent ways to sum the <c>int</c> elements out of a mixed-type array:
/// an <c>is</c> type-pattern in a loop, a <c>switch</c> statement, and a <c>switch</c>
/// expression via LINQ. All three should -- and, with each using its own accumulator, do --
/// agree.
/// </summary>
public static class SumPatternsDemo
{
    private static readonly object[] MixedData = [1, "two", 3, "four", 5];

    /// <summary>Runs all three summing variants and compares their results.</summary>
    public static Task RunAsync()
    {
        var viaIsPattern = SumWithIsPattern(MixedData);
        var viaSwitchStatement = SumWithSwitchStatement(MixedData);
        var viaSwitchExpression = SumWithSwitchExpression(MixedData);

        var agree = viaIsPattern == viaSwitchStatement && viaSwitchStatement == viaSwitchExpression;
        Console.WriteLine(
            $"Sum via is-pattern={viaIsPattern}  switch-statement={viaSwitchStatement}  " +
            $"switch-expression={viaSwitchExpression}  [{(agree ? "agree" : "DIFFER")}]");

        return Task.CompletedTask;
    }

    private static int SumWithIsPattern(object[] data)
    {
        var sum = 0;

        foreach (var item in data)
        {
            if (item is int intValue)
            {
                sum += intValue;
            }
        }

        return sum;
    }

    private static int SumWithSwitchStatement(object[] data)
    {
        var sum = 0;

        foreach (var item in data)
        {
            switch (item)
            {
                case int intValue:
                    sum += intValue;
                    break;
                case string textValue:
                    Console.WriteLine($"(switch statement) skipping non-numeric text: {textValue}");
                    break;
            }
        }

        return sum;
    }

    private static int SumWithSwitchExpression(object[] data) =>
        data.Sum(item => item switch
        {
            int intValue => intValue,
            _ => 0,
        });
}

/* Expected output
(switch statement) skipping non-numeric text: two
(switch statement) skipping non-numeric text: four
Sum via is-pattern=9  switch-statement=9  switch-expression=9  [agree]
*/
