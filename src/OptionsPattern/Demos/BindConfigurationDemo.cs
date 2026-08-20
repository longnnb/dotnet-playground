using Microsoft.Extensions.DependencyInjection;

namespace OptionsPattern.Demos;

/// <summary>The same binding as <see cref="BindFromJsonDemo"/>, via the fluent <c>AddOptions&lt;TOptions&gt;().BindConfiguration(sectionKey)</c> form instead.</summary>
public static class BindConfigurationDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.AddOptions<ReportOptions>().BindConfiguration(ReportOptions.SectionName);
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        return Task.CompletedTask;
    }
}
