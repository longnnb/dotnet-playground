using Microsoft.Extensions.DependencyInjection;

namespace Decorator.Demos;

/// <summary>
/// Demonstrates decorating generic handlers -- and a genuine, long-standing Scrutor limitation
/// this demo ran into while being written: an <em>open</em>-generic registration
/// (<c>services.AddTransient(typeof(IHandler&lt;&gt;), typeof(UpperCaseHandler&lt;&gt;))</c>)
/// cannot be decorated at all, neither via the open-generic form of <c>Decorate</c> nor via the
/// closed-generic form -- both throw <c>DecorationException</c> ("Could not find any registered
/// services"), because Scrutor matches by exact <c>ServiceType</c> and an open-generic
/// registration's <c>ServiceType</c> is never equal to any closed generic. See
/// <see href="https://github.com/khellang/Scrutor/issues/39">khellang/Scrutor#39</see> and
/// <see href="https://github.com/khellang/Scrutor/issues/49">#49</see>. The actual workaround,
/// used below: register the closed generics you need directly instead of one open-generic
/// registration, then decorate each closed registration.
/// </summary>
public static class OpenGenericDecorateDemo
{
    /// <summary>Runs the demo over two closed generic handlers, each registered and decorated individually.</summary>
    public static Task RunAsync()
    {
        var services = new ServiceCollection();

        // Closed-generic registrations, one per T actually needed -- the open-generic form
        // would be shorter, but (per the type's remarks) can't be decorated at all.
        services.AddTransient<IHandler<string>, UpperCaseHandler<string>>();
        services.AddTransient<IHandler<int>, UpperCaseHandler<int>>();

        services.Decorate<IHandler<string>, LoggingHandler<string>>();
        services.Decorate<IHandler<int>, LoggingHandler<int>>();

        using var provider = services.BuildServiceProvider();

        var stringHandler = provider.GetRequiredService<IHandler<string>>();
        Console.WriteLine(stringHandler.Handle("hello"));

        var intHandler = provider.GetRequiredService<IHandler<int>>();
        Console.WriteLine(intHandler.Handle(42));

        return Task.CompletedTask;
    }
}

file interface IHandler<T>
{
    string Handle(T value);
}

file sealed class UpperCaseHandler<T> : IHandler<T>
{
    public string Handle(T value) => value?.ToString()?.ToUpperInvariant() ?? "(null)";
}

file sealed class LoggingHandler<T>(IHandler<T> inner) : IHandler<T>
{
    public string Handle(T value)
    {
        Console.WriteLine($"[LoggingHandler<{typeof(T).Name}>] handling {value}");
        return inner.Handle(value);
    }
}

/* Expected output
[LoggingHandler<String>] handling hello
HELLO
[LoggingHandler<Int32>] handling 42
42
*/
