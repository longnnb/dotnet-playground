using Microsoft.Extensions.DependencyInjection;

namespace OptionsPattern.Demos;

/// <summary>Binds <see cref="ReportOptions"/> directly from the <c>ReportOptions</c> section of <c>appsettings.json</c> via <c>Configure&lt;TOptions&gt;(IConfigurationSection)</c>.</summary>
public static class BindFromJsonDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.Configure<ReportOptions>(builder.Configuration.GetSection(ReportOptions.SectionName));
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        return Task.CompletedTask;
    }
}
