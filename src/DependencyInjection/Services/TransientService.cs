namespace DependencyInjection.Services;

/// <summary>
/// A transient service: a new instance is created every time it's requested from the
/// container, whether within the same scope or not. Its <see cref="Id"/> exists purely to make
/// that per-resolution instance identity visible in demo output.
/// </summary>
public class TransientService
{
    /// <summary>The identity of this particular instance.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Prints this instance's id.</summary>
    public void PrintGuid() => Console.WriteLine($"Transient: {Id}");
}
