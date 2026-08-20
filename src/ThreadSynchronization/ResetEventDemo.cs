namespace ThreadSynchronization;

/// <summary>
/// Demonstrates <see cref="ManualResetEvent"/> and <see cref="AutoResetEvent"/>, whose entire
/// reason to exist alongside each other comes down to one difference in <c>Set()</c>: a manual
/// reset event releases <em>every</em> currently-waiting thread and stays open until explicitly
/// <c>Reset()</c>; an auto reset event releases exactly <em>one</em> waiting thread and resets
/// itself automatically. Both demos below make that difference directly observable.
/// </summary>
public static class ResetEventDemo
{
    /// <summary>
    /// One writer opens the gate once; all 5 reader threads that were waiting proceed together.
    /// Two fixes over an earlier version of this demo: <see cref="EventWaitHandle.Set"/> now
    /// runs in a <c>finally</c> (previously, an exception between starting the write and
    /// calling <c>Set()</c> would leave every reader blocked forever -- not merely broken, but
    /// hung), and the gate is reset once up front by the caller rather than inside the writer
    /// itself, which previously created a race: a second writer's <c>Reset()</c> could slam the
    /// gate shut again immediately after the first writer's <c>Set()</c>, stranding readers
    /// that hadn't woken yet.
    /// </summary>
    public static Task RunManualResetEventAsync()
    {
        using var gate = new ManualResetEvent(initialState: false);
        var readers = new Thread[5];

        for (var i = 0; i < readers.Length; i++)
        {
            readers[i] = new Thread(() => ReadWithManualResetEvent(gate));
            readers[i].Start();
        }

        Thread.Sleep(100); // give the readers a chance to start waiting
        WriteWithManualResetEvent(gate);

        foreach (var reader in readers)
        {
            reader.Join();
        }

        return Task.CompletedTask;
    }

    private static void WriteWithManualResetEvent(ManualResetEvent gate)
    {
        try
        {
            ThreadRunner.Log("writing...");
            Thread.Sleep(300);
            ThreadRunner.Log("writing completed -- opening the gate for every waiting reader.");
        }
        finally
        {
            gate.Set();
        }
    }

    private static void ReadWithManualResetEvent(ManualResetEvent gate)
    {
        ThreadRunner.Log("waiting for the gate to open...");
        gate.WaitOne();
        ThreadRunner.Log("reading...");
        Thread.Sleep(100);
        ThreadRunner.Log("reading completed.");
    }

    /// <summary>
    /// One writer signals three times; exactly one of 3 waiting reader threads is released per
    /// <see cref="EventWaitHandle.Set"/> call -- proving the "auto reset releases exactly one
    /// waiter" half of the two types' difference (contrast with
    /// <see cref="RunManualResetEventAsync"/>, where one <c>Set()</c> releases all of them).
    /// <see cref="EventWaitHandle.Set"/> also now runs in a <c>finally</c>, for the same reason
    /// as the manual-reset demo above.
    /// </summary>
    public static Task RunAutoResetEventAsync()
    {
        using var gate = new AutoResetEvent(initialState: false);
        var readers = new Thread[3];

        for (var i = 0; i < readers.Length; i++)
        {
            readers[i] = new Thread(() => ReadWithAutoResetEvent(gate));
            readers[i].Start();
        }

        Thread.Sleep(100); // give the readers a chance to start waiting
        WriteWithAutoResetEvent(gate);
        WriteWithAutoResetEvent(gate);
        WriteWithAutoResetEvent(gate);

        foreach (var reader in readers)
        {
            reader.Join();
        }

        return Task.CompletedTask;
    }

    private static void WriteWithAutoResetEvent(AutoResetEvent gate)
    {
        try
        {
            ThreadRunner.Log("signaling one waiting reader...");
            Thread.Sleep(100);
        }
        finally
        {
            gate.Set();
        }
    }

    private static void ReadWithAutoResetEvent(AutoResetEvent gate)
    {
        ThreadRunner.Log("waiting to be signaled...");
        gate.WaitOne();
        ThreadRunner.Log("signaled -- reading...");
        Thread.Sleep(100);
        ThreadRunner.Log("reading completed.");

        // Note: an AutoResetEvent can be Set() from any thread, including one that never called
        // WaitOne(). That flexibility is also a footgun -- prefer a Mutex (see MutexDemo) when
        // the "only the acquiring thread may release" invariant actually matters.
    }
}

/* Expected output (RunManualResetEventAsync) (thread ids and interleaving vary between runs;
   the invariant that always holds: every one of the 5 readers proceeds once the single Set()
   call opens the gate)
[T  A] waiting for the gate to open...
[T  B] waiting for the gate to open...
...
[T  W] writing...
[T  W] writing completed -- opening the gate for every waiting reader.
[T  A] reading...
[T  B] reading...
...
*/

/* Expected output (RunAutoResetEventAsync) (thread ids and interleaving vary between runs; the
   invariant that always holds: each Set() call releases exactly one reader, so with 3 readers
   and 3 Set() calls all 3 eventually get through, but never more than one per call)
[T  A] waiting to be signaled...
[T  B] waiting to be signaled...
[T  C] waiting to be signaled...
[T  W] signaling one waiting reader...
[T  A] signaled -- reading...
[T  W] signaling one waiting reader...
[T  B] signaled -- reading...
...
*/
