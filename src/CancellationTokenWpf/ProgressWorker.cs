namespace CancellationTokenWpf;

/// <summary>
/// The cancellable work itself, kept free of any WPF dependency so it reads clearly on its own.
/// Two variants show the two shapes cancellable work takes in practice.
/// </summary>
public static class ProgressWorker
{
    /// <summary>Number of steps the work reports progress for.</summary>
    public const int StepCount = 50;

    /// <summary>
    /// CPU-bound work: a synchronous loop that polls <paramref name="token"/> itself, meant to
    /// be run on a thread-pool thread via <c>Task.Run</c> so it doesn't block the UI thread.
    /// <see cref="CancellationToken.ThrowIfCancellationRequested"/> is called unconditionally at
    /// the top of each iteration -- it's a no-op when nothing has requested cancellation, so the
    /// separate <c>if (token.IsCancellationRequested)</c> guard an earlier version of this loop
    /// had (with a braceless body reading as empty apart from three comment lines) was
    /// redundant as well as confusing.
    /// </summary>
    public static void RunBlockingWork(IProgress<int> progress, CancellationToken token)
    {
        for (var i = 0; i <= StepCount; i++)
        {
            token.ThrowIfCancellationRequested();
            progress.Report(i);
            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// I/O-bound work: <c>await Task.Delay(..., token)</c> honors the token itself and throws
    /// <see cref="TaskCanceledException"/> the moment it's cancelled -- no manual polling, and
    /// no <c>Task.Run</c> needed, since nothing here blocks a thread while waiting. This is the
    /// better fit for what this demo's loop actually does (wait, then report), and it isn't
    /// shown anywhere else in this project.
    /// </summary>
    public static async Task RunAsyncWorkAsync(IProgress<int> progress, CancellationToken token)
    {
        for (var i = 0; i <= StepCount; i++)
        {
            await Task.Delay(100, token);
            progress.Report(i);
        }
    }
}
