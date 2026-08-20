using AsyncDisposable.Demos;

namespace AsyncDisposable;

/// <summary>
/// Entry point. With no arguments, runs every demo in this project in order; pass one or more
/// demo names to run only those, or <c>--list</c> to print the available names, e.g.
/// <c>dotnet run --project AsyncDisposable -- dispose-order</c>.
/// </summary>
internal static class Program
{
    private static readonly Dictionary<string, Func<Task>> Demos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["using-vs-await-using"] = UsingVsAwaitUsingDemo.RunAsync,
        ["exception-in-body"] = ExceptionInBodyDemo.RunAsync,
        ["exception-in-body-throw"] = ExceptionInBodyDemo.RunThrowingVariantAsync,
        ["dispose-order"] = DisposeOrderDemo.RunAsync,
        ["async-enumerable"] = AsyncEnumerableDemo.RunAsync,
        ["async-enumerable-early-break"] = AsyncEnumerableDemo.RunWithEarlyBreakAsync,
        ["async-scope"] = AsyncScopeDemo.RunAsync,
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
