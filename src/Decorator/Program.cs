using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Decorator;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Register the original service
        builder.Services.AddTransient<IMyService, MyService>();

        // Decorate the service using Scrutor
        builder.Services.Decorate<IMyService, MyServiceDecorator>();
        builder.Services.Decorate<IMyService, AnotherMyServiceDecorator>();

        using var host = builder.Build();

        var myservice = host.Services.GetRequiredService<IMyService>();
        myservice.Execute();
    }
}

public interface IMyService
{
    void Execute();
}

public class MyService : IMyService
{
    public void Execute()
    {
        Console.WriteLine("Executing MyService");
    }
}

public class MyServiceDecorator : IMyService
{
    private readonly IMyService _innerService;

    public MyServiceDecorator(IMyService innerService)
    {
        _innerService = innerService;
    }

    public void Execute()
    {
        // Pre-execution logic
        Console.WriteLine("Before executing MyService");

        // Call the original service
        _innerService.Execute();

        // Post-execution logic
        Console.WriteLine("After executing MyService");
    }
}

public class AnotherMyServiceDecorator : IMyService
{
    private readonly IMyService _innerService;

    public AnotherMyServiceDecorator(IMyService innerService)
    {
        _innerService = innerService;
    }

    public void Execute()
    {
        // Pre-execution logic
        Console.WriteLine("Before executing in AnotherMyServiceDecorator");

        // Call the original service
        _innerService.Execute();

        // Post-execution logic
        Console.WriteLine("After executing in AnotherMyServiceDecorator");
    }
}