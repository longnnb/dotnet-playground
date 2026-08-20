using Microsoft.Extensions.DependencyInjection;

namespace HttpClientTest.Demos;

/// <summary>Demonstrates the simplest <see cref="IHttpClientFactory"/> usage: <c>services.AddHttpClient()</c> with no name, then <c>CreateClient()</c> with no arguments.</summary>
public static class BasicClientDemo
{
    /// <summary>Runs the demo against a public, no-auth test endpoint.</summary>
    public static async Task RunAsync(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
        Console.WriteLine($"Body: {body}");
    }
}
