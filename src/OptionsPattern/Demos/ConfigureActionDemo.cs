using Microsoft.Extensions.DependencyInjection;

namespace OptionsPattern.Demos;

/// <summary>Configures via a plain <c>Action&lt;TOptions&gt;</c> lambda with hardcoded values -- no configuration or DI dependency involved at all.</summary>
public static class ConfigureActionDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.Configure<ReportOptions>(options =>
        {
            options.ConnectionString = "Custom connection from Action<TOptions>";
            options.Timeout = 20;
        });
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        return Task.CompletedTask;
    }
}
