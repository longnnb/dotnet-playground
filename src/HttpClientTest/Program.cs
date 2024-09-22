using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace HttpClientTest;

class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        ConfigureServices(builder.Services);
        
        using var host = builder.Build();
    }
    
    public static void ConfigureServices(IServiceCollection services)
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
        services.AddHttpClient<MyService>()
            .ConfigurePrimaryHttpMessageHandler<ProxyHttpClientHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        // Polly - Resilience policies like retry, circuit breaker, etc.
        services.AddHttpClient("RetryClient")
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
    }
}

public class ProxyHttpClientHandler : HttpClientHandler
{
    // private readonly IMyConfiguration _myConfiguration;

    public ProxyHttpClientHandler()
    {
        var proxy = new WebProxy
        {
            Address = new Uri("http://proxyserver:8080"),
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("username", "password")
        };
        
        Proxy = proxy;
        UseProxy = true;
    }
}

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log the request details
        Console.WriteLine($"Request: {request.Method} {request.RequestUri}");

        // Call the inner handler to send the request to the server
        var response = await base.SendAsync(request, cancellationToken);

        // Log the response details
        Console.WriteLine($"Response: {response.StatusCode}");

        return response;
    }
}
