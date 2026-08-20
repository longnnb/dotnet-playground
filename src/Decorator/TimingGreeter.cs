using System.Diagnostics;

namespace Decorator;

/// <summary>Decorates an <see cref="IGreeter"/> with elapsed-time reporting via <see cref="Stopwatch"/>.</summary>
public class TimingGreeter(IGreeter inner) : IGreeter
{
    /// <inheritdoc />
    public string Greet(string name)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = inner.Greet(name);
        stopwatch.Stop();
        Console.WriteLine($"[timing] Greet({name}) took {stopwatch.Elapsed.TotalMilliseconds:0.###}ms");
        return result;
    }
}
