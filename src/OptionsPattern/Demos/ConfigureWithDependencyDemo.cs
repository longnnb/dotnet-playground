using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OptionsPattern.Demos;

/// <summary>Configures via <c>AddOptions&lt;TOptions&gt;().Configure&lt;TDep&gt;((options, dep) =&gt; ...)</c>, injecting <see cref="IConfiguration"/> as the dependency instead of binding a section directly.</summary>
public static class ConfigureWithDependencyDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.AddOptions<ReportOptions>().Configure<IConfiguration>((options, configuration) =>
            configuration.GetSection(ReportOptions.SectionName).Bind(options));
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        return Task.CompletedTask;
    }
}
