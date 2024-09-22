using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestOption;

internal class Program
{
    private static IServiceScope scope;

    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddScoped<ScopedService>();
        builder.Services.AddScoped<SingletonService>();

        using var host = builder.Build();

        var scopedService1 = host.Services.GetRequiredService<ScopedService>();
        scopedService1?.PrintGuid();
        using (var scope = host.Services.CreateScope())
        {
            //var singletonService = scope.ServiceProvider.GetRequiredService<SingletonService>();
            //ScopedService scopedService1 = singletonService.scopedService;
            //var scopedService1 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            var scopedService3 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            var scopedService4 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            scopedService3?.PrintGuid();
            scopedService4?.PrintGuid();
        }

        //scopedService3?.PrintGuid();

        using (var scope = host.Services.CreateScope())
        {
            //var singletonService = scope.ServiceProvider.GetRequiredService<SingletonService>();
            //ScopedService scopedService2 = singletonService.scopedService;
            var scopedService2 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            scopedService2?.PrintGuid();
        }

        var scopedService5 = host.Services.GetRequiredService<ScopedService>();
        scopedService5?.PrintGuid();
        Console.ReadLine();
    }
}

public class ScopedService
{
    private readonly Guid guid = Guid.NewGuid();

    public void PrintGuid()
    {
        Console.WriteLine($"Scoped: {guid}");
    }
}

public class SingletonService
{
    public readonly ScopedService scopedService;

    public SingletonService(ScopedService scopedService)
    {
        this.scopedService = scopedService;
    }

    public void PrintGuid()
    {
        scopedService.PrintGuid();
    }
}