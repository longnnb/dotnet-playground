namespace OptionsPattern;

/// <summary>Consumes <see cref="ReportOptions"/> via <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/> and prints them.</summary>
public interface IOptionsConsumer
{
    /// <summary>Prints the options this consumer was given, plus its own identity (see <see cref="OptionsConsumer"/>).</summary>
    void PrintOptions();
}
