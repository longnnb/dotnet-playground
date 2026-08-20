using DependencyInjection.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Demos;

/// <summary>
/// Demonstrates that "singleton" means "singleton per <c>ServiceProvider</c>", not "singleton
/// per process" or "singleton per type": building two providers from the same
/// <see cref="ServiceCollection"/> gives each its own, independent singleton instance.
/// </summary>
public static class MultipleProvidersDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var services = new ServiceCollection();
        services.AddSingleton<SingletonService>();
        services.AddScoped<ScopedService>();
        services.AddTransient<TransientService>();

        using var providerA = services.BuildServiceProvider();
        using var providerB = services.BuildServiceProvider();

        var singletonFromA = providerA.GetRequiredService<SingletonService>();
        var singletonFromB = providerB.GetRequiredService<SingletonService>();

        Console.WriteLine($"Provider A's singleton id: {singletonFromA.Id}");
        Console.WriteLine($"Provider B's singleton id: {singletonFromB.Id}");
        Console.WriteLine(
            $"Same instance? {ReferenceEquals(singletonFromA, singletonFromB)} " +
            "(they can't be -- each provider owns its own singleton lifetime)");

        return Task.CompletedTask;
    }
}
