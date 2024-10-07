using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Task = System.Threading.Tasks.Task;

namespace HttpClientTest;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        ConfigureServices(builder.Services);

        using var host = builder.Build();

        var fhirService = host.Services.GetRequiredService<IFhirService>();

        var data = await fhirService.GetPatient("45069285");
        Console.WriteLine(data.Name[0].Family);
        // var response = await fhirService.GetPatients() as Bundle;
        // // fhirService.GetPatient("example");
        // Console.WriteLine(response.Total);
        // Console.WriteLine(response.Entry.Count);
        // Console.WriteLine(response.Entry[0].Resource.TypeName);
        await fhirService.CreatePatient();
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
        services.AddHttpClient<IMyService, MyService>()
            .ConfigurePrimaryHttpMessageHandler<ProxyHttpClientHandler>()
            .AddHttpMessageHandler<LoggingHandler>();

        // Polly - Resilience policies like retry, circuit breaker, etc.
        services.AddHttpClient("RetryClient")
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddHttpClient("FhirHttpClient")
            .AddHttpMessageHandler<LoggingHandler>();

        services.AddSingleton<IFhirService, FhirService>();
        services.AddTransient<LoggingHandler>();
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