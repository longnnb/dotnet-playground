using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace OptionsPattern.Configurers;

/// <summary>Binds <see cref="ReportOptions"/> from the <see cref="ReportOptions.SectionName"/> configuration section, as a standalone <see cref="IConfigureOptions{TOptions}"/> implementation.</summary>
public class ReportOptionsFromConfiguration(IConfiguration configuration) : IConfigureOptions<ReportOptions>
{
    /// <inheritdoc />
    public void Configure(ReportOptions options) => configuration.GetSection(ReportOptions.SectionName).Bind(options);
}
