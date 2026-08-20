using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OptionsPattern.Demos;

/// <summary>
/// Demonstrates the difference between <see cref="IOptions{TOptions}"/> (resolved once, frozen
/// for the lifetime of whatever holds it -- see <see cref="OptionsConsumer"/>) and
/// <see cref="IOptionsMonitor{TOptions}"/> (always reflects the latest bound value, and can
/// notify on change) when the underlying configuration changes at runtime. Uses
/// <see cref="ReloadableConfigurationProvider"/> -- a minimal custom provider, not
/// <c>appsettings.json</c> on disk -- so this demo has no file-system side effects.
/// </summary>
/// <remarks>
/// Two more obvious-looking approaches were tried while writing this demo and both turned out
/// not to work, which is why a custom provider is here at all:
/// <list type="bullet">
/// <item><description>
/// <c>new ConfigurationBuilder().AddInMemoryCollection(data).Build()</c>, then mutating the
/// same <c>data</c> dictionary and calling <c>((IConfigurationRoot)configuration).Reload()</c>:
/// the value read back from <see cref="IConfiguration"/> itself never changes, because
/// <c>AddInMemoryCollection</c> does not keep a live reference to the dictionary you pass it.
/// </description></item>
/// <item><description>
/// <see cref="ConfigurationManager"/>'s indexer setter (<c>configuration["key"] = "value"</c>):
/// this one DOES update the value -- reading it straight back from
/// <see cref="IConfiguration"/> shows the change immediately -- but <see cref="IOptionsMonitor{TOptions}"/>
/// never notices. <c>ConfigurationProvider.Set</c>, which the indexer calls into, only writes
/// to the provider's backing dictionary; it does not raise the change token that
/// <see cref="IOptionsMonitor{TOptions}"/> subscribes to. Only <c>ConfigurationProvider.OnReload()</c>
/// does that -- which is exactly what <see cref="ReloadableConfigurationProvider.SetAndReload"/>
/// below calls, the same way a real file-watching provider does after a file change.
/// </description></item>
/// </list>
/// </remarks>
public static class SnapshotVsMonitorDemo
{
    /// <summary>Runs the demo: reads the same option through both interfaces before and after a configuration change.</summary>
    public static Task RunAsync()
    {
        var configurationProvider = new ReloadableConfigurationProvider();
        configurationProvider.SetAndReload("ReportOptions:ConnectionString", "initial value");

        var configuration = new ConfigurationBuilder()
            .Add(new ReloadableConfigurationSource(configurationProvider))
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<ReportOptions>(configuration.GetSection(ReportOptions.SectionName));

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<ReportOptions>>();
        var monitor = provider.GetRequiredService<IOptionsMonitor<ReportOptions>>();

        Console.WriteLine($"Before change -- IOptions:        {options.Value.ConnectionString}");
        Console.WriteLine($"Before change -- IOptionsMonitor: {monitor.CurrentValue.ConnectionString}");

        var changeNotified = false;
        using var registration = monitor.OnChange(_ => changeNotified = true);

        configurationProvider.SetAndReload("ReportOptions:ConnectionString", "updated value");

        Console.WriteLine($"After change  -- IOptions:        {options.Value.ConnectionString} (frozen -- same instance, same value)");
        Console.WriteLine($"After change  -- IOptionsMonitor: {monitor.CurrentValue.ConnectionString} (reflects the change)");
        Console.WriteLine($"OnChange callback fired?          {changeNotified}");

        return Task.CompletedTask;
    }
}

/// <summary>
/// A minimal in-memory <see cref="ConfigurationProvider"/> whose <see cref="SetAndReload"/>
/// both updates a value and raises the reload token that <see cref="IOptionsMonitor{TOptions}"/>
/// listens for -- the piece plain <c>AddInMemoryCollection</c> and <see cref="ConfigurationManager"/>'s
/// indexer are missing for this scenario. This is the same two-step (write, then
/// <see cref="ConfigurationProvider.OnReload"/>) a real file-watching provider performs after
/// detecting a change on disk.
/// </summary>
public sealed class ReloadableConfigurationProvider : ConfigurationProvider
{
    /// <summary>Sets <paramref name="key"/> to <paramref name="value"/> and notifies every subscriber that configuration changed.</summary>
    public void SetAndReload(string key, string value)
    {
        Set(key, value);
        OnReload();
    }
}

/// <summary>An <see cref="IConfigurationSource"/> that always builds the same, externally-held <see cref="ReloadableConfigurationProvider"/> instance.</summary>
public sealed class ReloadableConfigurationSource(ReloadableConfigurationProvider provider) : IConfigurationSource
{
    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder) => provider;
}
