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
        var builder = Host.CreateApplicationBuilder(args);
        //builder.Configuration.AddJsonFile("appsettings.json");

        // get from appsettings.json
        //builder.Services.Configure<MyServiceOptions>(builder.Configuration.GetSection(nameof(MyServiceOptions)));
        //builder.Services.AddOptions<MyServiceOptions>().BindConfiguration("MyServiceOptions"); ;

        // add with IConfigureOptions<TOptions>
        //builder.Services.ConfigureOptions<MyServiceOptionsConfigurer>();
        //builder.Services.ConfigureOptions<MyServiceOptionsDatabaseConfigurer>();

        // add with provider
        // builder.Services.AddSingleton<IOptions<MyServiceOptions>>(serviceProvider =>
        // {
        //     var config = serviceProvider.GetService<IConfiguration>();
        //     var options = new MyServiceOptions();
        //     config?.GetSection(nameof(MyServiceOptions)).Bind(options);
        //     return Options.Create(options);
        // });

        // add using action
        // builder.Services.Configure<MyServiceOptions>(options =>
        // {
        //     options.ConnectionString = "Custom Connection from Action<TOptions>";
        //     options.Timeout = 20;
        // });
        //
        // add with dependency
        // builder.Services.AddOptions<MyServiceOptions>().Configure<IConfiguration>((options, configuration) =>
        // {
        //     configuration.GetSection(nameof(MyServiceOptions)).Bind(options);
        // });

        // add with dependency (from external source)
        // async not working
        builder.Services.AddOptions<MyServiceOptions>().Configure<IDataService>((options, dataService) =>
        {
            options.ConnectionString = dataService.GetConnectionString();
            options.Timeout = dataService.GetTimeout();
        });

        builder.Services.AddScoped<IDataService, DataService>();
        builder.Services.AddScoped<IMyService, MyService>();
        using var host = builder.Build();

        // test service
        var service = host.Services.GetService<IMyService>();
        service?.PrintOptions();

        // test configuration
        // var config = host.Services.GetService<IConfiguration>();
        // var op = config?.GetSection("MyServiceOptions").Get<MyServiceOptions>();
        // Console.WriteLine($"ConnectionString: {op?.ConnectionString}");
        // var test = config?.GetValue<string>("Test");
        // Console.WriteLine($"Test: {test}");
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