namespace ThreadSynchronization;

/// <summary>
/// Entry point. With no arguments, runs every demo in this project in order; pass one or more
/// demo names to run only those, or <c>--list</c> to print the available names, e.g.
/// <c>dotnet run --project ThreadSynchronization -- mutex</c>.
/// </summary>
/// <remarks>
/// Every demo joins its threads before returning (see <see cref="ThreadRunner.RunOnThreads"/>),
/// so output from different demos never interleaves and the whole run-all terminates cleanly.
/// </remarks>
internal static class Program
{
    private static readonly Dictionary<string, Func<Task>> Demos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["lock-object"] = LockDemo.RunWithObjectAsync,
        ["lock-typed"] = LockDemo.RunWithTypedLockAsync,
        ["monitor-enter-exit"] = MonitorDemo.RunWithEnterExitAsync,
        ["monitor-try-enter"] = MonitorDemo.RunWithTryEnterAsync,
        ["mutex"] = MutexDemo.RunAsync,
        ["mutex-named"] = MutexDemo.RunNamedMutexAsync,
        ["reset-event-manual"] = ResetEventDemo.RunManualResetEventAsync,
        ["reset-event-auto"] = ResetEventDemo.RunAutoResetEventAsync,
        ["semaphore-sync"] = SemaphoreDemo.RunSyncAsync,
        ["semaphore-async"] = SemaphoreDemo.RunAsyncAsync,
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
