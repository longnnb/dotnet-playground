using Microsoft.Extensions.DependencyInjection;

namespace Decorator.Demos;

/// <summary>
/// Demonstrates Scrutor's other headline feature -- <c>Scan()</c> -- which registers every
/// class in an assembly that implements a given interface, instead of one
/// <c>AddTransient&lt;IThing, Thing&gt;()</c> call per implementation.
/// </summary>
public static class AssemblyScanDemo
{
    /// <summary>Runs the demo: every <c>IPlugin</c> implementation in this assembly is discovered and registered automatically.</summary>
    public static Task RunAsync()
    {
        var services = new ServiceCollection();

        services.Scan(scan => scan
            .FromAssemblyOf<IPlugin>()
            // AddClasses's `publicOnly` parameter defaults to true, so it silently skips
            // non-public types -- including `file`-scoped ones like CsvExportPlugin/
            // PdfExportPlugin below. Pass false explicitly to scan those too; in a real
            // codebase, making the plugin types public is usually the more idiomatic fix.
            .AddClasses(classes => classes.AssignableTo<IPlugin>(), publicOnly: false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        using var provider = services.BuildServiceProvider();

        var plugins = provider.GetServices<IPlugin>();

        foreach (var plugin in plugins.OrderBy(p => p.Name))
        {
            Console.WriteLine($"Discovered plugin: {plugin.Name}");
        }

        return Task.CompletedTask;
    }
}

file interface IPlugin
{
    string Name { get; }
}

file sealed class CsvExportPlugin : IPlugin
{
    public string Name => "CSV export";
}

file sealed class PdfExportPlugin : IPlugin
{
    public string Name => "PDF export";
}

/* Expected output
Discovered plugin: CSV export
Discovered plugin: PDF export
*/
