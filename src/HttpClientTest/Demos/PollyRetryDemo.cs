using Microsoft.Extensions.DependencyInjection;

namespace HttpClientTest.Demos;

/// <summary>
/// Demonstrates the Polly v7-based retry policy (<c>AddTransientHttpErrorPolicy</c>) against
/// <see cref="Handlers.FailNTimesHandler"/>, which fails the first two requests then succeeds --
/// deterministic and fully offline, instead of depending on a remote server actually failing at
/// the right moment.
/// </summary>
public static class PollyRetryDemo
{
    /// <summary>Runs the demo.</summary>
    public static async Task RunAsync(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient("PollyRetryClient");

        var response = await client.GetAsync("https://example.test/data");
        Console.WriteLine($"Final status after retries: {(int)response.StatusCode} {response.StatusCode}");
    }
}
