namespace CSharpFeatures;

/// <summary>
/// Entry point. With no arguments, runs every demo in this project in order; pass one or more
/// demo names to run only those, or <c>--list</c> to print the available names, e.g.
/// <c>dotnet run --project CSharpFeatures -- tuples-equality</c>.
/// </summary>
internal static class Program
{
    private static readonly Dictionary<string, Func<Task>> Demos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["out-variables"] = OutVariablesDemo.RunAsync,
        ["out-variables-scope"] = OutVariablesDemo.RunScopeAsync,
        ["out-discard-and-dictionary"] = OutVariablesDemo.RunDiscardAndDictionaryAsync,
        ["culture-pitfall"] = OutVariablesDemo.RunCulturePitfallAsync,
        ["sum-patterns"] = SumPatternsDemo.RunAsync,
        ["shape-patterns"] = ShapePatternsDemo.RunAsync,
        ["relational-patterns"] = RelationalPatternsDemo.RunAsync,
        ["list-patterns"] = ListPatternsDemo.RunAsync,
        ["tuples-basics"] = TuplesDemo.RunBasicsAsync,
        ["tuples-vs-out"] = TuplesDemo.RunTupleVsOutParametersAsync,
        ["tuples-deconstruction"] = TuplesDemo.RunDeconstructionAsync,
        ["tuples-equality"] = TuplesDemo.RunEqualityAsync,
        ["tuples-dictionary-keys"] = TuplesDemo.RunAsDictionaryKeysAsync,
        ["tuples-name-erasure"] = TuplesDemo.RunNameErasureAsync,
        ["tuples-linq"] = TuplesDemo.RunInLinqAsync,
        ["tuples-vs-record"] = TuplesDemo.RunTupleVsRecordAsync,
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
