namespace ThreadSynchronization;

/// <summary>
/// Demonstrates <see cref="SemaphoreSlim"/> limiting concurrency to 2 at a time, both
/// synchronously (<see cref="SemaphoreSlim.Wait()"/>) and asynchronously
/// (<see cref="SemaphoreSlim.WaitAsync()"/> -- present in an earlier version of this project but,
/// unlike the synchronous path, never actually invoked by anything).
/// </summary>
/// <remarks>
/// <c>initialCount</c> is how many callers can enter without blocking right after construction;
/// <c>maximumCount</c> is the highest the internal counter can reach -- <c>Release()</c> throws
/// once the counter would exceed it. With <c>initialCount == maximumCount</c> (as here), calling
/// <c>Release()</c> before any matching <c>Wait()</c> would throw immediately.
/// </remarks>
public static class SemaphoreDemo
{
    private const int MaxConcurrency = 2;
    private static readonly SemaphoreSlim Semaphore = new(MaxConcurrency, MaxConcurrency);

    /// <summary>Runs 5 threads through the synchronous <see cref="SemaphoreSlim.Wait()"/>/<see cref="SemaphoreSlim.Release()"/> path, never more than 2 concurrently.</summary>
    public static Task RunSyncAsync()
    {
        ThreadRunner.RunOnThreads(5, DoWork);
        return Task.CompletedTask;
    }

    /// <summary>Runs 5 async workers through <see cref="SemaphoreSlim.WaitAsync()"/> concurrently via <see cref="Task.WhenAll(IEnumerable{Task})"/>, never more than 2 concurrently.</summary>
    public static async Task RunAsyncAsync()
    {
        var workers = Enumerable.Range(0, 5).Select(_ => DoWorkAsync());
        await Task.WhenAll(workers);
    }

    private static void DoWork()
    {
        ThreadRunner.Log("waiting for the semaphore...");
        Semaphore.Wait();

        try
        {
            ThreadRunner.Log($"acquired -- {Semaphore.CurrentCount} slot(s) left, working...");
            Thread.Sleep(300);
            ThreadRunner.Log("done.");
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static async Task DoWorkAsync()
    {
        ThreadRunner.Log("waiting for the semaphore (async)...");
        await Semaphore.WaitAsync();

        try
        {
            ThreadRunner.Log($"acquired (async) -- {Semaphore.CurrentCount} slot(s) left, working...");
            await Task.Delay(300);
            ThreadRunner.Log("done (async).");
        }
        finally
        {
            Semaphore.Release();
        }
    }
}

/* Expected output (thread ids and interleaving vary between runs; the invariant that always
   holds: at most 2 workers report "acquired" before either of them reports "done")
[T  X] waiting for the semaphore...
[T  X] acquired -- 1 slot(s) left, working...
[T  Y] waiting for the semaphore...
[T  Y] acquired -- 0 slot(s) left, working...
[T  Z] waiting for the semaphore...
[T  X] done.
[T  Z] acquired -- 0 slot(s) left, working...
...
*/
