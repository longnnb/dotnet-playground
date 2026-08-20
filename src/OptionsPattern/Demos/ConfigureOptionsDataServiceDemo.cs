using Microsoft.Extensions.DependencyInjection;
using OptionsPattern.Configurers;

namespace OptionsPattern.Demos;

/// <summary>Binds via the standalone <see cref="ReportOptionsFromDataService"/> class registered with <c>ConfigureOptions&lt;T&gt;()</c> -- the class-based counterpart to <see cref="ConfigureFromDataServiceDemo"/>'s inline lambda.</summary>
public static class ConfigureOptionsDataServiceDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.AddScoped<IDataService, DataService>();
        builder.Services.ConfigureOptions<ReportOptionsFromDataService>();
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        return Task.CompletedTask;
    }
}
