namespace DependencyInjection.Services;

/// <summary>
/// A scoped service: one instance per <see cref="Microsoft.Extensions.DependencyInjection.IServiceScope"/>,
/// shared by everything resolved within that scope, but a fresh instance in the next scope.
/// </summary>
public class ScopedService(TransientService transientService)
{
    /// <summary>The identity of this particular instance.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Prints this instance's id alongside the transient service it depends on.</summary>
    public void PrintGuid()
    {
        Console.WriteLine($"Scoped: {Id}");
        Console.WriteLine($"  Transient in Scoped: {transientService.Id}");
    }
}
