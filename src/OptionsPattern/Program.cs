using OptionsPattern.Demos;

namespace OptionsPattern;

/// <summary>
/// Entry point. With no arguments, runs every demo in this project in order; pass one or more
/// demo names to run only those, or <c>--list</c> to print the available names, e.g.
/// <c>dotnet run --project OptionsPattern -- async-configuration</c>.
/// </summary>
internal static class Program
{
    private static readonly Dictionary<string, Func<Task>> Demos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bind-from-json"] = BindFromJsonDemo.RunAsync,
        ["bind-configuration"] = BindConfigurationDemo.RunAsync,
        ["configure-options"] = ConfigureOptionsDemo.RunAsync,
        ["configure-options-data-service"] = ConfigureOptionsDataServiceDemo.RunAsync,
        ["configure-with-dependency"] = ConfigureWithDependencyDemo.RunAsync,
        ["configure-from-data-service"] = ConfigureFromDataServiceDemo.RunAsync,
        ["options-provider"] = OptionsProviderDemo.RunAsync,
        ["configure-action"] = ConfigureActionDemo.RunAsync,
        ["validation"] = ValidationDemo.RunAsync,
        ["snapshot-vs-monitor"] = SnapshotVsMonitorDemo.RunAsync,
        ["async-configuration"] = AsyncConfigurationDemo.RunAsync,
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
    }
}
