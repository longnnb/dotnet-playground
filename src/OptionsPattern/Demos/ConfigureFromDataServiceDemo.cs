using Microsoft.Extensions.DependencyInjection;

namespace OptionsPattern.Demos;

/// <summary>Configures from a dependency that has nothing to do with <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> -- <see cref="IDataService"/> -- via an inline <c>AddOptions&lt;TOptions&gt;().Configure&lt;TDep&gt;(...)</c> lambda.</summary>
public static class ConfigureFromDataServiceDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.AddScoped<IDataService, DataService>();
        builder.Services.AddOptions<ReportOptions>().Configure<IDataService>((options, dataService) =>
        {
            options.ConnectionString = dataService.GetConnectionString();
            options.Timeout = dataService.GetTimeout();
        });
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        return Task.CompletedTask;
    }
}
