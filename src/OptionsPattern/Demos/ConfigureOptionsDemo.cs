using Microsoft.Extensions.DependencyInjection;
using OptionsPattern.Configurers;

namespace OptionsPattern.Demos;

/// <summary>Binds via the standalone <see cref="ReportOptionsFromConfiguration"/> class registered with <c>ConfigureOptions&lt;T&gt;()</c>, instead of a lambda or fluent call.</summary>
public static class ConfigureOptionsDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.ConfigureOptions<ReportOptionsFromConfiguration>();
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        return Task.CompletedTask;
    }
}
