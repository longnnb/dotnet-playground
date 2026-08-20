namespace DesignPatterns.Structural;

/// <summary>
/// Adapter: converts one interface into another that client code expects, without changing
/// either side -- here, wrapping a legacy string-only logger so it can be used wherever the
/// newer structured-logging interface is expected.
/// </summary>
public static class AdapterDemo
{
    /// <summary>Runs the demo: the call site logs through the adapter without knowing the legacy type exists.</summary>
    public static Task RunAsync()
    {
        IStructuredLogger logger = new LegacyLoggerAdapter(new LegacyFileLogger());
        logger.Log(LogSeverity.Warning, "disk space low");
        logger.Log(LogSeverity.Info, "startup complete");

        return Task.CompletedTask;
    }
}

file enum LogSeverity
{
    Info,
    Warning,
}

file interface IStructuredLogger
{
    void Log(LogSeverity severity, string message);
}

/// <summary>The interface client code is already written against -- this can't be changed.</summary>
file sealed class LegacyFileLogger
{
    public void WriteLine(string text) => Console.WriteLine($"[legacy] {text}");
}

/// <summary>Adapts <see cref="LegacyFileLogger"/>'s single <c>WriteLine(string)</c> method to <see cref="IStructuredLogger"/>'s <c>Log(severity, message)</c>.</summary>
file sealed class LegacyLoggerAdapter(LegacyFileLogger inner) : IStructuredLogger
{
    public void Log(LogSeverity severity, string message) =>
        inner.WriteLine($"{severity.ToString().ToUpperInvariant()}: {message}");
}

/* Expected output
[legacy] WARNING: disk space low
[legacy] INFO: startup complete
*/
