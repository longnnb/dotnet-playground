using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OptionsPattern.Demos;

/// <summary>
/// Demonstrates <c>ValidateDataAnnotations().ValidateOnStart()</c>: binding a source that
/// violates <see cref="ReportOptions"/>'s <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/>
/// constraint fails fast, during host startup, instead of producing a silently-invalid options
/// instance the first time something calls <c>.Value</c>.
/// </summary>
public static class ValidationDemo
{
    /// <summary>Runs both halves: a valid section that starts cleanly, and an invalid one whose host fails to start.</summary>
    public static async Task RunAsync()
    {
        await RunWithValidSectionAsync();
        await RunWithInvalidSectionAsync();
    }

    private static async Task RunWithValidSectionAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.AddOptions<ReportOptions>()
            .BindConfiguration(ReportOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        using var host = builder.Build();
        await host.StartAsync();

        var options = host.Services.GetRequiredService<IOptions<ReportOptions>>().Value;
        Console.WriteLine($"Valid section started cleanly: {options}");

        await host.StopAsync();
    }

    private static async Task RunWithInvalidSectionAsync()
    {
        var builder = DemoHost.CreateBuilder();

        // No section bound at all: ConnectionString stays "" ([Required] fails); Timeout stays
        // at its default of 30, which is valid on its own -- only the ConnectionString
        // violation is what trips validation here.
        builder.Services.AddOptions<ReportOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        using var host = builder.Build();

        try
        {
            await host.StartAsync();
            Console.WriteLine("Invalid section: unexpectedly started cleanly.");
            await host.StopAsync();
        }
        catch (OptionsValidationException ex)
        {
            Console.WriteLine($"Invalid section: failed fast at startup -- {string.Join("; ", ex.Failures)}");
        }
    }
}
