using DependencyInjection.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DependencyInjection.Demos;

/// <summary>
/// Demonstrates the three built-in service lifetimes -- transient, scoped, singleton -- by
/// resolving each from two different <see cref="IServiceScope"/>s and comparing instance
/// identities: singleton is the same everywhere, scoped is the same within a scope but
/// different across scopes, transient is different every time.
/// </summary>
public static class LifetimeDemo
{
    /// <summary>Runs the demo over two scopes and prints the identity relationships between them.</summary>
    public static Task RunAsync()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddTransient<TransientService>();
        builder.Services.AddScoped<ScopedService>();
        builder.Services.AddSingleton<SingletonService>();

        using var host = builder.Build();

        var a = DescribeScope(host.Services, "Scope A");
        var b = DescribeScope(host.Services, "Scope B");

        Console.WriteLine();
        Console.WriteLine($"Scoped same within a scope?    {a.Scoped1 == a.Scoped2} (and {b.Scoped1 == b.Scoped2})");
        Console.WriteLine($"Scoped same across scopes?     {a.Scoped1 == b.Scoped1}");
        Console.WriteLine($"Transient ever the same?       {a.Transient1 == a.Transient2}");
        Console.WriteLine($"Singleton same across scopes?  {a.Singleton == b.Singleton}");

        return Task.CompletedTask;
    }

    private static (Guid Singleton, Guid Scoped1, Guid Scoped2, Guid Transient1, Guid Transient2) DescribeScope(
        IServiceProvider root, string label)
    {
        using var scope = root.CreateScope();
        var provider = scope.ServiceProvider;

        Console.WriteLine($"-- {label} --");

        var singleton = provider.GetRequiredService<SingletonService>();
        singleton.PrintGuid();

        var scoped1 = provider.GetRequiredService<ScopedService>();
        var scoped2 = provider.GetRequiredService<ScopedService>();
        scoped1.PrintGuid();
        scoped2.PrintGuid();

        var transient1 = provider.GetRequiredService<TransientService>();
        var transient2 = provider.GetRequiredService<TransientService>();
        transient1.PrintGuid();
        transient2.PrintGuid();

        return (singleton.Id, scoped1.Id, scoped2.Id, transient1.Id, transient2.Id);
    }
}
