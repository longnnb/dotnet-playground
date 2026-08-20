using Microsoft.Extensions.DependencyInjection;

namespace HttpClientTest.Demos;

/// <summary>
/// Demonstrates the named-client pattern: <c>services.AddHttpClient("Todo", client => ...)</c>
/// then <c>factory.CreateClient("Todo")</c> by string name. This is the pattern the deleted
/// <c>MyController</c> class existed to show -- except that class was never actually
/// instantiated by anything, 100% dead code in a console app with no ASP.NET host to discover a
/// "Controller" in the first place.
/// </summary>
public static class NamedClientDemo
{
    /// <summary>Runs the demo.</summary>
    public static async Task RunAsync(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient("Todo");

        var response = await client.GetAsync("/todos/2");
        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
        Console.WriteLine($"Body: {body}");
    }
}
