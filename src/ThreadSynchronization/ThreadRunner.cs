namespace ThreadSynchronization;

/// <summary>
/// Two small helpers used by every demo in this project: a log line prefixed with the current
/// managed thread id, and a way to spawn N threads running the same body and <em>join them all
/// before returning</em>. An earlier version of this project spawned threads and let
/// <c>Main</c> return without joining them -- harmless for a program that exits right after,
/// but fatal for a dispatcher that runs several demos back to back, since a straggling thread
/// from one demo would go on printing into the next demo's output.
/// </summary>
internal static class ThreadRunner
{
    /// <summary>Writes <paramref name="message"/> prefixed with the current managed thread id.</summary>
    public static void Log(string message) => Console.WriteLine($"[T{Environment.CurrentManagedThreadId,3}] {message}");

    /// <summary>Starts <paramref name="count"/> threads running <paramref name="body"/> and blocks until every one of them finishes.</summary>
    public static void RunOnThreads(int count, Action body)
    {
        var threads = new Thread[count];

        for (var i = 0; i < count; i++)
        {
            threads[i] = new Thread(() => body());
            threads[i].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
    }
}
