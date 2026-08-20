namespace DesignPatterns.Behavioral;

/// <summary>
/// Strategy: extracts an algorithm into an interchangeable object so it can vary independently
/// of the code that uses it -- here, shipping cost calculation. Run two ways: as classic
/// interface-based strategy objects, and as plain <see cref="Func{T,TResult}"/> delegates,
/// which is often the lighter-weight C# equivalent when a strategy is just one method.
/// </summary>
public static class StrategyDemo
{
    /// <summary>Runs both strategy styles over the same weight and prints both results.</summary>
    public static Task RunAsync()
    {
        IShippingStrategy[] objectStrategies = [new GroundShipping(), new OvernightShipping()];

        foreach (var strategy in objectStrategies)
        {
            Console.WriteLine($"{strategy.Name} (object strategy): {strategy.CalculateCost(weightKg: 4.5):0.00}");
        }

        Dictionary<string, Func<double, decimal>> funcStrategies = new()
        {
            ["Ground"] = weightKg => 5.00m + (decimal)weightKg * 0.75m,
            ["Overnight"] = weightKg => 20.00m + (decimal)weightKg * 1.50m,
        };

        foreach (var (name, calculate) in funcStrategies)
        {
            Console.WriteLine($"{name} (Func strategy): {calculate(4.5):0.00}");
        }

        return Task.CompletedTask;
    }
}

file interface IShippingStrategy
{
    // Deliberately not using GetType().Name here: `file`-scoped types get a compiler-mangled
    // runtime name, which would leak into output that's supposed to be a clean comparison.
    string Name { get; }

    decimal CalculateCost(double weightKg);
}

file sealed class GroundShipping : IShippingStrategy
{
    public string Name => nameof(GroundShipping);

    public decimal CalculateCost(double weightKg) => 5.00m + (decimal)weightKg * 0.75m;
}

file sealed class OvernightShipping : IShippingStrategy
{
    public string Name => nameof(OvernightShipping);

    public decimal CalculateCost(double weightKg) => 20.00m + (decimal)weightKg * 1.50m;
}

/* Expected output
GroundShipping (object strategy): 8.38
OvernightShipping (object strategy): 26.75
Ground (Func strategy): 8.38
Overnight (Func strategy): 26.75
*/
