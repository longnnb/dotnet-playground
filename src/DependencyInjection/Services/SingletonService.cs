using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Services;

/// <summary>
/// A singleton service: exactly one instance for the lifetime of the container. It depends on
/// <see cref="IServiceScopeFactory"/> rather than directly on <see cref="ScopedService"/> --
/// see <c>DependencyInjection.Demos.CaptiveDependencyDemo</c> for why injecting a scoped
/// service straight into a singleton's constructor is a bug, not just a style choice.
/// </summary>
public class SingletonService(IServiceScopeFactory scopeFactory)
{
    /// <summary>The identity of this particular instance -- the same value no matter how many times or where it's resolved.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Creates its own short-lived scope to reach scoped state, rather than holding a scoped
    /// dependency captive for its own, much longer, lifetime.
    /// </summary>
    public void PrintGuid()
    {
        using var scope = scopeFactory.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<ScopedService>();

        Console.WriteLine($"Singleton: {Id}");
        Console.WriteLine($"  Scoped (from a scope this singleton created itself): {scopedService.Id}");
    }
}
