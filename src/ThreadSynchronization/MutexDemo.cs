namespace ThreadSynchronization;

/// <summary>
/// Demonstrates <see cref="Mutex"/>: an OS-level mutual-exclusion primitive that, unlike
/// <see cref="ManualResetEvent"/>/<see cref="AutoResetEvent"/> (see <see cref="ResetEventDemo"/>),
/// enforces thread ownership -- only the thread that acquired it may release it -- and, when
/// named, can coordinate across process boundaries.
/// </summary>
public static class MutexDemo
{
    /// <summary>
    /// Runs 5 threads contending for one local <see cref="Mutex"/>. Two fixes over an earlier
    /// version of this demo: <see cref="Mutex.ReleaseMutex"/> now runs in a <c>finally</c>
    /// (previously it didn't -- any exception between acquiring and releasing would abandon the
    /// mutex, and the next waiter would get an <see cref="AbandonedMutexException"/> instead of
    /// the original error), and <see cref="WaitHandle.WaitOne(TimeSpan)"/> now has a real timeout
    /// (previously the parameterless overload was used, which blocks until acquired and
    /// therefore always returns <see langword="true"/> -- the "failed to acquire" branch could
    /// never actually run).
    /// </summary>
    public static Task RunAsync()
    {
        using var mutex = new Mutex(initiallyOwned: false);
        ThreadRunner.RunOnThreads(5, () => DoWork(mutex));

        return Task.CompletedTask;
    }

    private static void DoWork(Mutex mutex)
    {
        ThreadRunner.Log("waiting for the mutex...");
        var acquired = false;

        try
        {
            acquired = mutex.WaitOne(TimeSpan.FromMilliseconds(50));

            if (acquired)
            {
                ThreadRunner.Log("acquired the mutex, working...");
                Thread.Sleep(200);
                ThreadRunner.Log("done.");
            }
            else
            {
                ThreadRunner.Log("failed to acquire the mutex within 50ms.");
            }
        }
        finally
        {
            if (acquired)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    /// <summary>
    /// The other reason <see cref="Mutex"/> exists alongside <c>lock</c>/<see cref="Monitor"/>:
    /// a <em>named</em> mutex is visible across process boundaries, so it can enforce "only one
    /// instance of this application at a time" -- something no in-process primitive can do.
    /// Prefixing the name with <c>Global\</c> makes it visible across all sessions on the
    /// machine, not just the current login session.
    /// </summary>
    public static Task RunNamedMutexAsync()
    {
        using var namedMutex = new Mutex(initiallyOwned: false, @"Global\DotNetPlaygroundThreadSyncDemo", out var createdNew);

        Console.WriteLine(createdNew
            ? "This process created the named mutex -- it's the only holder right now."
            : "Another process (or an earlier, still-running instance of this one) already holds this named mutex.");

        return Task.CompletedTask;
    }
}

/* Expected output (RunAsync) (thread ids and the exact success/timeout split vary between runs;
   with a 200ms hold time against a 50ms wait, typically one thread succeeds and the rest time out)
[T  X] waiting for the mutex...
[T  X] acquired the mutex, working...
[T  Y] waiting for the mutex...
[T  Y] failed to acquire the mutex within 50ms.
...
[T  X] done.
*/

/* Expected output (RunNamedMutexAsync)
This process created the named mutex -- it's the only holder right now.
*/
