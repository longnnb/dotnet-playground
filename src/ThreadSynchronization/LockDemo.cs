namespace ThreadSynchronization;

/// <summary>
/// Demonstrates the <c>lock</c> statement -- C#'s syntax sugar over <see cref="Monitor"/> --
/// first over a plain <see cref="object"/> (the only option before .NET 9) and then over
/// <see cref="Lock"/> (.NET 9+), a purpose-built mutual-exclusion type that <c>lock</c>
/// recognizes and compiles more efficiently against.
/// </summary>
public static class LockDemo
{
    private static readonly object ObjectGate = new();
    private static readonly Lock TypedGate = new();

    /// <summary>Runs 5 threads contending for a plain-<see cref="object"/> lock.</summary>
    public static Task RunWithObjectAsync()
    {
        ThreadRunner.RunOnThreads(5, DoWorkWithObjectLock);
        return Task.CompletedTask;
    }

    /// <summary>Runs 5 threads contending for a <see cref="Lock"/> via its explicit <see cref="Lock.EnterScope"/> API.</summary>
    public static Task RunWithTypedLockAsync()
    {
        ThreadRunner.RunOnThreads(5, DoWorkWithTypedLock);
        return Task.CompletedTask;
    }

    private static void DoWorkWithObjectLock()
    {
        lock (ObjectGate)
        {
            ThreadRunner.Log("working (object lock)...");
            Thread.Sleep(200);
            ThreadRunner.Log("done (object lock).");
        }
    }

    private static void DoWorkWithTypedLock()
    {
        // `lock (TypedGate)` also works and compiles to the same EnterScope/Dispose pattern --
        // spelled out explicitly here to show what the `lock` sugar is actually doing.
        using (TypedGate.EnterScope())
        {
            ThreadRunner.Log("working (Lock.EnterScope)...");
            Thread.Sleep(200);
            ThreadRunner.Log("done (Lock.EnterScope).");
        }
    }
}

/* Expected output (thread ids and interleaving vary between runs; the invariant that always
   holds: every thread runs its section to completion, one at a time, never interleaved)
[T  X] working (object lock)...
[T  X] done (object lock).
[T  Y] working (object lock)...
[T  Y] done (object lock).
... (repeats for all 5 threads)
*/
