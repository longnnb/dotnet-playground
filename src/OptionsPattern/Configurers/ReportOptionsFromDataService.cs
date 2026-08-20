using Microsoft.Extensions.Options;

namespace OptionsPattern.Configurers;

/// <summary>Configures <see cref="ReportOptions"/> from <see cref="IDataService"/> -- an external, non-configuration source -- as a standalone <see cref="IConfigureOptions{TOptions}"/> implementation.</summary>
public class ReportOptionsFromDataService(IDataService dataService) : IConfigureOptions<ReportOptions>
{
    /// <inheritdoc />
    public void Configure(ReportOptions options)
    {
        options.ConnectionString = dataService.GetConnectionString();
        options.Timeout = dataService.GetTimeout();
    }
}
