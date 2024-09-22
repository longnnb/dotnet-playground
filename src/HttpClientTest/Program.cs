using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace HttpClientTest;

class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        

        using var host = builder.Build();
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        // Basic HttpClient registration
        services.AddHttpClient();

        // Named HttpClient
        services.AddHttpClient("MyClient", client =>
        {
            client.BaseAddress = new Uri("https://api.example.com");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Typed HttpClient
        services.AddHttpClient<MyService>();

        // Polly - Resilience policies like retry, circuit breaker, etc.
        services.AddHttpClient("RetryClient")
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    }
}