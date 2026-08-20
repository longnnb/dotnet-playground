namespace DesignPatterns.Creational;

/// <summary>
/// Singleton: ensures a type has exactly one instance, reachable from a well-known access
/// point, built lazily and thread-safely via <see cref="Lazy{T}"/>.
/// </summary>
/// <remarks>
/// This is the classic textbook shape, and it is almost never the right answer in a codebase
/// that already has a DI container: <c>services.AddSingleton&lt;T&gt;()</c> (see the
/// <c>DependencyInjection</c> project) gives the same "exactly one instance" guarantee while
/// keeping the type testable -- you can register a fake for tests -- and keeping its lifetime
/// explicit and container-managed instead of baked into the type itself. Reach for this classic
/// form only when there genuinely is no container in play.
/// </remarks>
public static class SingletonDemo
{
    /// <summary>Runs the demo: two accesses to the singleton return the same instance.</summary>
    public static Task RunAsync()
    {
        var first = AppConfiguration.Instance;
        var second = AppConfiguration.Instance;

        Console.WriteLine($"Same instance both times? {ReferenceEquals(first, second)}");
        Console.WriteLine($"first.InstanceId == second.InstanceId: {first.InstanceId == second.InstanceId}");

        return Task.CompletedTask;
    }
}

file sealed class AppConfiguration
{
    private static readonly Lazy<AppConfiguration> LazyInstance = new(() => new AppConfiguration());

    public static AppConfiguration Instance => LazyInstance.Value;

    public Guid InstanceId { get; } = Guid.NewGuid();

    private AppConfiguration()
    {
    }
}

/* Expected output
Same instance both times? True
first.InstanceId == second.InstanceId: True
*/
