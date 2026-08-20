using System.Net;
using System.Text.Json;
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

        var data = await fhirService.GetResource<Resource>("45077913");
        Console.WriteLine(JsonSerializer.Serialize(data));
        // var data = await fhirService.UpdatePatient("45069285");
        // Console.WriteLine(JsonSerializer.Serialize(data));
        // var data1 = await fhirService.GetResource<Patient>("45069287");
        // Console.WriteLine(JsonSerializer.Serialize(data1));
        // var data2 = await fhirService.GetResource<Patient>("example");
        // Console.WriteLine(JsonSerializer.Serialize(data2));
        // var response = await fhirService.GetPatients() as Bundle;
        // // fhirService.GetPatient("example");
        // Console.WriteLine(response.Total);
        // Console.WriteLine(response.Entry.Count);
        // Console.WriteLine(response.Entry[0].Resource.TypeName);
        // var patient = await fhirService.CreatePatient();
        // Console.WriteLine(JsonSerializer.Serialize(patient));
        
        //45077913
        //45077914
        //45077915
        //45077916
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