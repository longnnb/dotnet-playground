using Microsoft.Extensions.Logging;

namespace Decorator;

/// <summary>Decorates an <see cref="IGreeter"/> with entry/exit logging via an injected <see cref="ILogger{TCategoryName}"/>.</summary>
public class LoggingGreeter(IGreeter inner, ILogger<LoggingGreeter> logger) : IGreeter
{
    /// <inheritdoc />
    public string Greet(string name)
    {
        logger.LogInformation("Greeting {Name}", name);
        var result = inner.Greet(name);
        logger.LogInformation("Greeted {Name}: {Result}", name, result);
        return result;
    }
}
