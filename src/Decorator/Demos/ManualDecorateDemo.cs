using Microsoft.Extensions.Logging.Abstractions;

namespace Decorator.Demos;

/// <summary>
/// The same composition as <see cref="ScrutorDecorateDemo"/>, hand-wired with plain
/// constructor calls instead of a container -- what Scrutor's <c>Decorate</c> calls are doing
/// underneath.
/// </summary>
public static class ManualDecorateDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        IGreeter greeter = new TimingGreeter(new LoggingGreeter(new Greeter(), NullLogger<LoggingGreeter>.Instance));
        Console.WriteLine(greeter.Greet("Ada"));

        return Task.CompletedTask;
    }
}

/* Expected output (the elapsed-ms value varies)
[timing] Greet(Ada) took 0.088ms
Hello, Ada!
*/
