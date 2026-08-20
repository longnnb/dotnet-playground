using DesignPatterns.Behavioral;
using DesignPatterns.Creational;
using DesignPatterns.Structural;

namespace DesignPatterns;

/// <summary>
/// Entry point. With no arguments, runs every pattern demo in this project in order; pass one
/// or more demo names to run only those, or <c>--list</c> to print the available names, e.g.
/// <c>dotnet run --project DesignPatterns -- strategy</c>.
/// </summary>
/// <remarks>
/// Decorator is deliberately not represented here -- it has its own project (see
/// <c>Decorator</c>) with a DI-container-based demo that this project would only duplicate.
/// </remarks>
internal static class Program
{
    private static readonly Dictionary<string, Func<Task>> Demos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["factory-method"] = FactoryMethodDemo.RunAsync,
        ["builder"] = BuilderDemo.RunAsync,
        ["singleton"] = SingletonDemo.RunAsync,
        ["adapter"] = AdapterDemo.RunAsync,
        ["composite"] = CompositeDemo.RunAsync,
        ["facade"] = FacadeDemo.RunAsync,
        ["strategy"] = StrategyDemo.RunAsync,
        ["observer"] = ObserverDemo.RunAsync,
        ["command"] = CommandDemo.RunAsync,
    };

    private static async Task Main(string[] args)
    {
        if (args is ["--list"])
        {
            foreach (var name in Demos.Keys)
            {
                Console.WriteLine(name);
            }

            return;
        }

        var namesToRun = args.Length == 0 ? [.. Demos.Keys] : args;

        foreach (var name in namesToRun)
        {
            if (!Demos.TryGetValue(name, out var run))
            {
                Console.Error.WriteLine($"Unknown demo '{name}'. Try --list.");
                Environment.ExitCode = 1;
                continue;
            }

            Console.WriteLine($"===== {name} =====");
            await run();
            Console.WriteLine();
        }

        if (args.Length == 0)
        {
            Console.WriteLine("Decorator: see the Decorator project (dotnet run --project Decorator) for a DI-container-based decorator demo.");
        }
    }
}
