using Microsoft.Extensions.Options;

namespace OptionsPattern;

/// <summary>
/// <see cref="Id"/> exists to make one specific trade-off visible: this consumer captures
/// <c>options.Value</c> once, in its constructor, which is exactly what keeps
/// <see cref="IOptionsMonitor{TOptions}"/>'s live-reload from reaching it -- contrast with
/// <see cref="Demos.SnapshotVsMonitorDemo"/>, which reads through
/// <see cref="IOptionsMonitor{TOptions}"/> instead and does see updates.
/// </summary>
public class OptionsConsumer(IOptions<ReportOptions> options) : IOptionsConsumer
{
    private readonly ReportOptions _options = options.Value;

    /// <summary>This instance's identity, to make its resolution lifetime visible in output.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public void PrintOptions() => Console.WriteLine($"Consumer {Id}: {_options}");
}
