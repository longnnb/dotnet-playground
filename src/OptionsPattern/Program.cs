using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TestOption;

internal class Program
{
    private static IServiceScope scope;

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var builder = Host.CreateApplicationBuilder(args);
        //builder.Configuration.AddJsonFile("appsettings.json");
        //builder.Services.ConfigureOptions<MyServiceOptionsConfigurer>();
        //builder.Services.Configure<MyServiceOptions>(builder.Configuration.GetSection(nameof(MyServiceOptions)));
        //builder.Services.AddOptions<MyServiceOptions>().BindConfiguration("MyServiceOptions"); ;
        //builder.Services.AddScoped<IMyService, MyService>();
        //builder.Services.AddScoped<IDataService, DataService>();
        // builder.Services.AddConfiguredService(options =>
        // {
        //     options.ConnectionString = "CustomConnection";
        //     options.Timeout = 20;
        // });
        //builder.Services.AddConfiguredServiceFromConfiguration();

        //builder.Services.AddConfiguredServiceFromConfiguration();
        //builder.Services.AddConfiguredServiceFromDatabase();

        using var host = builder.Build();

        // var config = host.Services.GetService<IConfiguration>();
        // var op = config?.GetSection("MyServiceOptions");

        //var service = host.Services.GetService<IMyService>();

        //var config = host.Services.GetService<IConfiguration>();
        //var op = config?.GetSection("MyServiceOptions").Get<MyServiceOptions>();
        //var test = config?.GetValue<string>("Test");

        //service?.PrintOptions();

        //var serviceCollection = new ServiceCollection();
        //serviceCollection.AddOptions<MyServiceOptions>().BindConfiguration("MyServiceOptions");
        //serviceCollection.AddSingleton<IMyService>(x => new MyService());
        //var serviceProvider = serviceCollection.BuildServiceProvider();


        //var service = serviceProvider.GetService<IMyService>();
        //var service3 = serviceProvider.GetService<IMyService>();
        //service?.PrintOptions();
        //service3?.PrintOptions();
        //var serviveProvider2 = serviceCollection.BuildServiceProvider();
        //var service2 = serviveProvider2.GetService<IMyService>();
        //service2?.PrintOptions();
        //using (scope = serviceProvider.CreateScope())
        //{
        //    var service4 = scope.ServiceProvider.GetService<IMyService>();
        //    service4?.PrintOptions();
        //    CallService();
        //}
    }

    private static void CallService()
    {
        var service5 = scope.ServiceProvider.GetService<IMyService>();
        service5?.PrintOptions();
    }
}

public class MyServiceOptionsConfigurer : IConfigureOptions<MyServiceOptions>
{
    private readonly IConfiguration configuration;

    public MyServiceOptionsConfigurer(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void Configure(MyServiceOptions options)
    {
        configuration.GetSection(nameof(MyServiceOptions)).Bind(options);
    }
}

public class MyServiceOptionsDatabaseConfigurer : IConfigureOptions<MyServiceOptions>
{
    private readonly IDataService dataService;

    public MyServiceOptionsDatabaseConfigurer(IDataService dataService)
    {
        this.dataService = dataService;
    }

    public void Configure(MyServiceOptions options)
    {
        options.ConnectionString = dataService.GetConnectionString();
        options.Timeout = dataService.GetTimeout();
    }
}

public static class MyServiceExtensions
{
    public static IServiceCollection AddConfiguredService(this IServiceCollection services,
        Action<MyServiceOptions> configureOptions)
    {
        // Configure the options using the provided action
        services.Configure(configureOptions);
        // Add the service to the service collection
        services.AddScoped<IMyService, MyService>();

        return services;
    }

    public static IServiceCollection AddConfiguredServiceFromConfiguration(this IServiceCollection services)
    {
        // Use Configure to load options from appsettings.json
        //services.Configure<MyServiceOptions>(configuration.GetSection("MyServiceOptions"));

        // Add the data service and the service to the service collection
        // services.ConfigureOptions<MyServiceOptionsConfigurer>();
        // services.AddOptions<MyServiceOptions>().Configure<IConfiguration>((options, configuration) =>
        // {
        //     configuration.GetSection(nameof(MyServiceOptions)).Bind(options);
        // });

        services.AddSingleton<IOptions<MyServiceOptions>>(serviceProvider =>
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var options = new MyServiceOptions();
            config?.GetSection(nameof(MyServiceOptions)).Bind(options);
            return Options.Create(options);
        });

        services.AddScoped<IMyService, MyService>();

        return services;
    }

    public static IServiceCollection AddConfiguredServiceFromDatabase(this IServiceCollection services)
    {
        // services.ConfigureOptions<MyServiceOptionsDatabaseConfigurer>();
        services.AddOptions<MyServiceOptions>().Configure<IDataService>((options, dataService) =>
        {
            options.ConnectionString = dataService.GetConnectionString();
            options.Timeout = dataService.GetTimeout();
        });
        services.AddScoped<IMyService, MyService>();

        return services;
    }
}

public interface IDataService
{
    string GetConnectionString();
    int GetTimeout();
}

public class DataService : IDataService
{
    public string GetConnectionString()
    {
        // Simulate fetching the connection string from a data service
        return "ConnectionStringFromDataService";
    }

    public int GetTimeout()
    {
        // Simulate fetching the timeout value from a data service
        return 25;
    }
}

public class Exercise
{
    public static int FindMax(int[,] numbers)
    {
        if (numbers.GetLength(0) == 0 || numbers.GetLength(1) == 0) return -1;
        var max = numbers[0, 0];
        for (var i = 0; i < numbers.GetLength(0); i++)
        for (var j = 0; j < numbers.GetLength(1); j++)
            if (numbers[i, j] > max)
                max = numbers[i, j];
        return max;
    }
}