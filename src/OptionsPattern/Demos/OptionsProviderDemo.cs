using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OptionsPattern.Demos;

/// <summary>Supplies <see cref="IOptions{TOptions}"/> directly via a provider lambda, bypassing the Configure/AddOptions pipeline entirely.</summary>
public static class OptionsProviderDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.AddSingleton<IOptions<ReportOptions>>(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var options = new ReportOptions();
            configuration.GetSection(ReportOptions.SectionName).Bind(options);
            return Options.Create(options);
        });
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        return Task.CompletedTask;
    }
}
