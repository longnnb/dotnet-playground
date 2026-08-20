using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OptionsPattern.Demos;

/// <summary>
/// Demonstrates why <see cref="IDataService.GetConnectionStringAsync"/>/<see cref="IDataService.GetTimeoutAsync"/>
/// exist but nothing in <see cref="Configurers.ReportOptionsFromDataService"/> or
/// <see cref="ConfigureFromDataServiceDemo"/> ever calls them: the options configuration
/// pipeline has no async hook. <c>IConfigureOptions&lt;TOptions&gt;.Configure</c> is a
/// synchronous <see langword="void"/> method by design, so there's no <c>await</c> to be had
/// inside it -- and the tempting fix is actively dangerous.
/// </summary>
public static class AsyncConfigurationDemo
{
    /// <summary>Runs all four parts: the constraint, the wrong fix, and the two right answers.</summary>
    public static async Task RunAsync()
    {
        ShowTheConstraint();
        await ShowTheWrongFixAsync();
        await ShowPreloadingBeforeFirstResolutionAsync();
        await ShowOptionsMonitorWithReloadAsync();
    }

    private static void ShowTheConstraint()
    {
        Console.WriteLine(
            "IConfigureOptions<TOptions>.Configure(TOptions options) returns void -- there is " +
            "no async overload in the options pipeline to opt into, by design.");
    }

    /// <summary>
    /// The tempting-but-wrong fix: block on the async call with <c>.GetAwaiter().GetResult()</c>
    /// inside a synchronous <c>Configure</c> delegate. This demo's host has no
    /// <see cref="SynchronizationContext"/> to deadlock on -- console apps don't have one; it's
    /// ASP.NET Core's classic context that made this pattern infamous -- so it happens to
    /// complete here. On a host that does have a capturing context, this exact shape deadlocks.
    /// </summary>
    private static async Task ShowTheWrongFixAsync()
    {
        var builder = DemoHost.CreateBuilder();
        builder.Services.AddScoped<IDataService, DataService>();
        builder.Services.AddOptions<ReportOptions>().Configure<IDataService>((options, dataService) =>
        {
            // GetAwaiter().GetResult() blocks the calling thread until the async call
            // completes. Safe here only because this host has no SynchronizationContext to
            // deadlock against -- do not copy this into one that does.
            options.ConnectionString = dataService.GetConnectionStringAsync().GetAwaiter().GetResult();
        });
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();

        await Task.CompletedTask;
    }

    /// <summary>
    /// The first correct answer: do the async work before anything resolves
    /// <see cref="IOptions{TOptions}"/> for the first time, and hand the already-awaited result
    /// to a synchronous <c>Configure</c> delegate.
    /// </summary>
    private static async Task ShowPreloadingBeforeFirstResolutionAsync()
    {
        var dataService = new DataService();
        var connectionString = await dataService.GetConnectionStringAsync();
        var timeout = await dataService.GetTimeoutAsync();

        var builder = DemoHost.CreateBuilder();
        builder.Services.Configure<ReportOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.Timeout = timeout;
        });
        builder.Services.AddScoped<IOptionsConsumer, OptionsConsumer>();

        using var host = builder.Build();
        host.Services.GetRequiredService<IOptionsConsumer>().PrintOptions();
    }

    /// <summary>
    /// The second correct answer: bind synchronously at startup for an immediately-usable
    /// placeholder, then push the real value into the backing configuration once the async
    /// fetch completes -- consumers that read through <see cref="IOptionsMonitor{TOptions}"/>
    /// (not <see cref="IOptions{TOptions}"/>, which freezes -- see
    /// <see cref="SnapshotVsMonitorDemo"/>) see the update. Uses
    /// <see cref="ReloadableConfigurationProvider"/> (see <see cref="SnapshotVsMonitorDemo"/>'s
    /// remarks for why a plain in-memory source doesn't work for this).
    /// </summary>
    private static async Task ShowOptionsMonitorWithReloadAsync()
    {
        var configurationProvider = new ReloadableConfigurationProvider();
        configurationProvider.SetAndReload("ReportOptions:ConnectionString", "placeholder until async load completes");

        var configuration = new ConfigurationBuilder()
            .Add(new ReloadableConfigurationSource(configurationProvider))
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<ReportOptions>(configuration.GetSection(ReportOptions.SectionName));

        using var provider = services.BuildServiceProvider();
        var monitor = provider.GetRequiredService<IOptionsMonitor<ReportOptions>>();

        Console.WriteLine($"Immediately usable value:        {monitor.CurrentValue.ConnectionString}");

        var dataService = new DataService();
        var connectionString = await dataService.GetConnectionStringAsync();
        configurationProvider.SetAndReload("ReportOptions:ConnectionString", connectionString);

        Console.WriteLine($"After the async fetch completes: {monitor.CurrentValue.ConnectionString}");
    }
}
