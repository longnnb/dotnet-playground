using Microsoft.Extensions.Options;

namespace TestOption;

public class MyService : IMyService
{
    private readonly MyServiceOptions _options;
    private readonly Guid guid = Guid.NewGuid();

    public MyService(IOptions<MyServiceOptions> options)
    {
        _options = options.Value;
    }

    public void PrintOptions()
    {
        Console.WriteLine($"Service: {guid}");
        Console.WriteLine($"ConnectionString: {_options.ConnectionString}");
        Console.WriteLine($"Timeout: {_options.Timeout}");
        Console.WriteLine($"IntValue: {_options.IntValue}");
        Console.WriteLine($"StringValue: {_options.StringValue}");
        Console.WriteLine($"BoolValue: {_options.BoolValue}");
    }
}