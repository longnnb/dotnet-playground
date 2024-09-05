using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DependencyInjection;

class Program
{
    // private static IServiceScope scope;

    static void Main(string[] args)
    {
        // var services = new ServiceCollection();
        // services.AddTransient<TransientService>();
        // services.AddScoped<ScopedService>();
        // services.AddSingleton<SingletonService>();
        // var provider1 = services.BuildServiceProvider();
        // var provider2 = services.BuildServiceProvider();
        // var singletonService1 = provider1.GetRequiredService<SingletonService>();
        // var singletonService2 = provider2.GetRequiredService<SingletonService>();
        // singletonService1.PrintGuid();
        // singletonService2.PrintGuid();
        
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddTransient<TransientService>();
        builder.Services.AddScoped<ScopedService>();
        builder.Services.AddSingleton<SingletonService>();

        using IHost host = builder.Build();

        // var scopedService1 = host.Services.GetRequiredService<ScopedService>();
        // scopedService1?.PrintGuid();
        // var transientService1 = host.Services.GetRequiredService<TransientService>();
        // transientService1?.PrintGuid();
        // var singletonService1 = host.Services.GetRequiredService<SingletonService>();
        // singletonService1?.PrintGuid();

        using (var scope = host.Services.CreateScope())
        {
            var singletonService = scope.ServiceProvider.GetRequiredService<SingletonService>();
            singletonService?.PrintGuid();
            var scopedService2 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            var scopedService3 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            // var scopedService4 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            scopedService2?.PrintGuid();
            scopedService3?.PrintGuid();
            // scopedService4?.PrintGuid();
            var transientService2 = scope.ServiceProvider.GetRequiredService<TransientService>();
            transientService2.PrintGuid();
            var transientService3 = scope.ServiceProvider.GetRequiredService<TransientService>();
            transientService3.PrintGuid();
        }

        using (var scope = host.Services.CreateScope())
        {
            var singletonService = scope.ServiceProvider.GetRequiredService<SingletonService>();
            singletonService?.PrintGuid();
            var scopedService2 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            var scopedService3 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            // var scopedService4 = scope.ServiceProvider.GetRequiredService<ScopedService>();
            scopedService2?.PrintGuid();
            scopedService3?.PrintGuid();
            // scopedService4?.PrintGuid();
            var transientService2 = scope.ServiceProvider.GetRequiredService<TransientService>();
            transientService2.PrintGuid();
            var transientService3 = scope.ServiceProvider.GetRequiredService<TransientService>();
            transientService3.PrintGuid();
        }


        // var scopedService5 = host.Services.GetRequiredService<ScopedService>();
        // scopedService5?.PrintGuid();
        // var transientService4 = host.Services.GetRequiredService<TransientService>();
        // transientService4?.PrintGuid();

        Console.ReadLine();
    }
}

public class SingletonService
{
    private readonly Guid guid = Guid.NewGuid();
    private readonly ScopedService scopedService;
    private readonly TransientService transientService;

    public SingletonService(ScopedService scopedService, TransientService transientService)
    {
        this.scopedService = scopedService;
        this.transientService = transientService;
    }

    public void PrintGuid()
    {
        Console.WriteLine($"Singleton: {guid}");
        Console.WriteLine($"Scoped in Singleton: {scopedService.GetGuid()}");
        Console.WriteLine($"Transient in Singleton: {transientService.GetGuid()}");
        Console.WriteLine("==============================");
    }

    public Guid GetGuid() => guid;
}

public class ScopedService
{
    private readonly Guid guid = Guid.NewGuid();
    private readonly TransientService transientService;
    private readonly SingletonService singletonService;

    // public ScopedService(TransientService transientService, SingletonService singletonService)
    // {
    //     this.transientService = transientService;
    //     this.singletonService = singletonService;
    // }
    
    public ScopedService(TransientService transientService)
    {
        this.transientService = transientService;
    }

    public void PrintGuid()
    {
        Console.WriteLine($"Scoped: {guid}");
        // Console.WriteLine($"Singleton in Scoped: {singletonService.GetGuid()}");
        Console.WriteLine($"Transient in Scoped: {transientService.GetGuid()}");
        Console.WriteLine("============================");
    }

    public Guid GetGuid() => guid;
}

public class TransientService
{
    private readonly Guid guid = Guid.NewGuid();
    private readonly ScopedService scopedService;
    private readonly SingletonService singletonService;

    // public TransientService(ScopedService scopedService, SingletonService singletonService)
    // {
    //     this.scopedService = scopedService;
    //     this.singletonService = singletonService;
    // }
    
    public void PrintGuid()
    {
        Console.WriteLine($"Transient: {guid}");
        // Console.WriteLine($"Singleton in Transient: {singletonService.GetGuid()}");
        // Console.WriteLine($"Scoped in Transient: {scopedService.GetGuid()}");
        Console.WriteLine("============================");
    }

    public Guid GetGuid() => guid;
}