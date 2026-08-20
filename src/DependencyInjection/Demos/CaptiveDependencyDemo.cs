using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Demos;

/// <summary>
/// Demonstrates the captive dependency problem: constructor-injecting a scoped service
/// directly into a singleton. The singleton is built once and holds onto whatever scoped
/// instance it received forever, defeating the point of "scoped" -- and whether that mistake
/// is caught immediately or corrupts state silently depends entirely on a validation flag
/// that's off by default in most hosts.
/// </summary>
public static class CaptiveDependencyDemo
{
    /// <summary>Runs both halves of the demo: validation catching the mistake, then validation off letting it through silently.</summary>
    public static Task RunAsync()
    {
        RunWithValidation();
        RunWithoutValidation();

        return Task.CompletedTask;
    }

    private static void RunWithValidation()
    {
        var services = new ServiceCollection();
        services.AddScoped<CapturedScopedService>();
        services.AddSingleton<CapturingSingleton>();

        try
        {
            // ValidateOnBuild makes the container walk every registration's dependency graph
            // right here, in the constructor -- the exception below is thrown by
            // BuildServiceProvider itself, not by a later GetRequiredService call.
            using var provider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true,
            });

            Console.WriteLine("ValidateScopes=true, ValidateOnBuild=true: unexpectedly succeeded.");
        }
        catch (AggregateException ex) when (ex.InnerException is InvalidOperationException inner)
        {
            Console.WriteLine($"ValidateScopes=true, ValidateOnBuild=true: caught the mistake immediately -- {inner.Message}");
        }
    }

    private static void RunWithoutValidation()
    {
        var services = new ServiceCollection();
        services.AddScoped<CapturedScopedService>();
        services.AddSingleton<CapturingSingleton>();

        // ValidateScopes defaults to true only in the Development environment under
        // Host.CreateApplicationBuilder -- this mirrors what every other environment sees.
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = false,
        });

        var singleton = provider.GetRequiredService<CapturingSingleton>();
        var capturedAtConstruction = singleton.Captured.Id;

        using var scope = provider.CreateScope();
        var scopedHere = scope.ServiceProvider.GetRequiredService<CapturedScopedService>();

        Console.WriteLine(
            $"ValidateScopes=false: no exception. The singleton's captured instance " +
            $"({capturedAtConstruction}) is NOT the same as a fresh scope's instance " +
            $"({scopedHere.Id}) -- the singleton is silently stuck with whatever scope built it.");
    }
}

// Deliberately not `file`-scoped: both type names leak into the DI container's validation
// exception message below, and a `file` type's compiler-mangled name would make that message
// unreadable -- the same issue hit (and fixed the same way) in AsyncDisposable's AsyncScopeDemo.
internal sealed class CapturedScopedService
{
    public Guid Id { get; } = Guid.NewGuid();
}

internal sealed class CapturingSingleton(CapturedScopedService captured)
{
    public CapturedScopedService Captured { get; } = captured;
}
