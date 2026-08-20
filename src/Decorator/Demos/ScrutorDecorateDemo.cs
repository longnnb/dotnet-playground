using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Decorator.Demos;

/// <summary>
/// Demonstrates Scrutor's <c>Decorate&lt;TService, TDecorator&gt;()</c>: registering an
/// interface's decorators one call at a time and letting the container wire up the wrapping.
/// </summary>
/// <remarks>
/// The rule this demo makes visible: <strong>the last <c>Decorate</c> call becomes the
/// outermost wrapper.</strong> Registering <see cref="LoggingGreeter"/> then
/// <see cref="TimingGreeter"/> means the runtime call order is
/// <c>TimingGreeter -&gt; LoggingGreeter -&gt; Greeter</c>, not the other way around --
/// the timing measurement ends up wrapping the logging calls too.
/// </remarks>
public static class ScrutorDecorateDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddTransient<IGreeter, Greeter>();
        builder.Services.Decorate<IGreeter, LoggingGreeter>();
        builder.Services.Decorate<IGreeter, TimingGreeter>();

        using var host = builder.Build();

        var greeter = host.Services.GetRequiredService<IGreeter>();
        Console.WriteLine(greeter.Greet("Ada"));

        return Task.CompletedTask;
    }
}

/* Expected output (the exact "info:" console-logging format may vary by .NET version; the
   elapsed-ms value always varies)
info: Decorator.LoggingGreeter[0]
      Greeting Ada
info: Decorator.LoggingGreeter[0]
      Greeted Ada: Hello, Ada!
[timing] Greet(Ada) took 8.234ms
Hello, Ada!
*/
