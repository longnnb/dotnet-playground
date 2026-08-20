namespace ThreadSynchronization;

/// <summary>
/// Demonstrates <see cref="Monitor"/> directly -- the primitive <c>lock</c> compiles down to --
/// including the <c>lockTaken</c> pattern that makes <see cref="Monitor.Enter(object, ref bool)"/>/<see cref="Monitor.Exit(object)"/>
/// exception-safe, and <see cref="Monitor.TryEnter(object, TimeSpan, ref bool)"/> with a real
/// timeout so its failure branch is actually reachable.
/// </summary>
public static class MonitorDemo
{
    private static readonly object Gate = new();

    /// <summary>Runs 5 threads using <see cref="Monitor.Enter(object, ref bool)"/>/<see cref="Monitor.Exit(object)"/> directly.</summary>
    public static Task RunWithEnterExitAsync()
    {
        ThreadRunner.RunOnThreads(5, DoWorkWithMonitor);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Runs 5 threads contending with a 50ms wait against a 200ms hold time, so most calls to
    /// <see cref="Monitor.TryEnter(object, TimeSpan, ref bool)"/> genuinely time out instead of
    /// racing to succeed.
    /// </summary>
    public static Task RunWithTryEnterAsync()
    {
        ThreadRunner.RunOnThreads(5, DoWorkWithTryEnter);
        return Task.CompletedTask;
    }

    /// <summary>
    /// The correct <c>lockTaken</c> pattern: <see cref="Monitor.Enter(object, ref bool)"/> is
    /// called <em>outside</em> the <c>try</c>. An earlier version of this demo called it
    /// <em>inside</em> the <c>try</c> -- if <c>Enter</c> had thrown or been aborted before
    /// acquiring the lock, the <c>finally</c> would still call <see cref="Monitor.Exit(object)"/>
    /// on a lock this thread never owned, throwing <see cref="SynchronizationLockException"/>
    /// and masking whatever the original problem was.
    /// </summary>
    private static void DoWorkWithMonitor()
    {
        var lockTaken = false;

        try
        {
            Monitor.Enter(Gate, ref lockTaken);
            ThreadRunner.Log("working (Monitor.Enter/Exit)...");
            Thread.Sleep(200);
            ThreadRunner.Log("done (Monitor.Enter/Exit).");
        }
        finally
        {
            if (lockTaken)
            {
                Monitor.Exit(Gate);
            }
        }
    }

    private static void DoWorkWithTryEnter()
    {
        var lockTaken = false;

        try
        {
            Monitor.TryEnter(Gate, TimeSpan.FromMilliseconds(50), ref lockTaken);

            if (lockTaken)
            {
                ThreadRunner.Log("acquired the lock within 50ms, working...");
                Thread.Sleep(200);
                ThreadRunner.Log("done (TryEnter).");
            }
            else
            {
                ThreadRunner.Log("failed to acquire the lock within 50ms.");
            }
        }
        finally
        {
            if (lockTaken)
            {
                Monitor.Exit(Gate);
            }
        }
    }
}

/* Expected output (RunWithEnterExitAsync) (thread ids and interleaving vary between runs)
[T  X] working (Monitor.Enter/Exit)...
[T  X] done (Monitor.Enter/Exit).
... (repeats for all 5 threads, one at a time)
*/

/* Expected output (RunWithTryEnterAsync) (thread ids, and exactly which threads time out, vary
   between runs; the invariant that always holds: since the hold time (200ms) exceeds the wait
   timeout (50ms), typically only the first thread to arrive succeeds and the rest time out)
[T  X] acquired the lock within 50ms, working...
[T  Y] failed to acquire the lock within 50ms.
[T  Z] failed to acquire the lock within 50ms.
[T  X] done (TryEnter).
...
*/
