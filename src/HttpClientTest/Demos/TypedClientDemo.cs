using Microsoft.Extensions.DependencyInjection;

namespace HttpClientTest.Demos;

/// <summary>Demonstrates the typed-client pattern: <see cref="ITodoClient"/>/<see cref="TodoClient"/>, resolved directly instead of through <see cref="IHttpClientFactory"/> by name.</summary>
public static class TypedClientDemo
{
    /// <summary>Runs the demo.</summary>
    public static async Task RunAsync(IServiceProvider services)
    {
        var todoClient = services.GetRequiredService<ITodoClient>();
        var body = await todoClient.GetTodoAsync(3);

        Console.WriteLine($"Body: {body}");
    }
}
