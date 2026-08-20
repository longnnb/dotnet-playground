using Microsoft.Extensions.DependencyInjection;

namespace HttpClientTest.Demos;

/// <summary>
/// Demonstrates <c>Microsoft.Extensions.Http.Resilience</c>'s <c>AddStandardResilienceHandler()</c>
/// -- the Polly-v8-based successor to <see cref="PollyRetryDemo"/>'s Polly-v7 policy -- against
/// the same kind of deterministic, offline failure sequence.
/// </summary>
public static class StandardResilienceDemo
{
    /// <summary>Runs the demo.</summary>
    public static async Task RunAsync(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IHttpClientFactory>();
        using var client = factory.CreateClient("StandardResilienceClient");

        var response = await client.GetAsync("https://example.test/data");
        Console.WriteLine($"Final status after the resilience pipeline: {(int)response.StatusCode} {response.StatusCode}");
    }
}
