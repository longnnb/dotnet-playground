using HttpClientTest.Demos;
using HttpClientTest.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace HttpClientTest;

/// <summary>
/// Entry point. Builds one shared host with every HttpClient/typed-client/FHIR registration
/// this project demonstrates, then dispatches by demo name. With no arguments, runs every
/// read-only demo in order; pass one or more demo names to run only those -- including the two
/// FHIR-writing demos, which never run by default (see <see cref="FhirDemos"/>) -- or
/// <c>--list</c> to print the available names, e.g.
/// <c>dotnet run --project HttpClientTest -- fhir-create</c>.
/// </summary>
internal static class Program
{
    private static readonly Dictionary<string, Func<IServiceProvider, Task>> AllDemos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["basic-client"] = BasicClientDemo.RunAsync,
        ["named-client"] = NamedClientDemo.RunAsync,
        ["typed-client"] = TypedClientDemo.RunAsync,
        ["polly-retry"] = PollyRetryDemo.RunAsync,
        ["standard-resilience"] = StandardResilienceDemo.RunAsync,
        ["fhir-search"] = FhirDemos.RunSearchAsync,
        ["fhir-read"] = FhirDemos.RunReadAsync,
        ["fhir-create"] = FhirDemos.RunCreateAsync,
        ["fhir-update"] = FhirDemos.RunUpdateAsync,
    };

    // fhir-create/fhir-update write to a shared public server -- opt-in only, never part of the
    // default run-all triggered by `dotnet run` with no arguments.
    private static readonly string[] DefaultDemos =
    [
        "basic-client", "named-client", "typed-client",
        "polly-retry", "standard-resilience", "fhir-search", "fhir-read",
    ];

    private static async Task Main(string[] args)
    {
        if (args is ["--list"])
        {
            foreach (var name in AllDemos.Keys)
            {
                Console.WriteLine(name);
            }

            return;
        }

        var builder = Host.CreateApplicationBuilder(args);
        ConfigureServices(builder.Services, builder.Configuration);
        using var host = builder.Build();

        var namesToRun = args.Length == 0 ? DefaultDemos : args;

        foreach (var name in namesToRun)
        {
            if (!AllDemos.TryGetValue(name, out var run))
            {
                Console.Error.WriteLine($"Unknown demo '{name}'. Try --list.");
                Environment.ExitCode = 1;
                continue;
            }

            Console.WriteLine($"===== {name} =====");
            await run(host.Services);
            Console.WriteLine();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ProxyOptions>().BindConfiguration(ProxyOptions.SectionName);

        services.AddTransient<LoggingHandler>();
        services.AddTransient<FhirHeadersHandler>();
        services.AddTransient<ProxyHttpClientHandler>();

        // Basic, unnamed client.
        services.AddHttpClient();

        // Named client (see NamedClientDemo -- replaces the never-instantiated MyController).
        services.AddHttpClient("Todo", client => client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com"))
            .AddHttpMessageHandler<LoggingHandler>();

        // Typed client. The proxy handler is only wired in when explicitly enabled (see
        // ProxyOptions.Enabled) -- an earlier version of this registration wired it in
        // unconditionally with nothing registering ProxyHttpClientHandler in DI at all, which
        // crashed the moment anything actually resolved the typed client.
        var typedClientBuilder = services
            .AddHttpClient<ITodoClient, TodoClient>(client => client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com"))
            .AddHttpMessageHandler<LoggingHandler>();

        var proxyEnabled = configuration.GetValue<bool>($"{ProxyOptions.SectionName}:{nameof(ProxyOptions.Enabled)}");

        if (proxyEnabled)
        {
            typedClientBuilder.ConfigurePrimaryHttpMessageHandler<ProxyHttpClientHandler>();
        }

        // Polly (v7) retry policy against a deterministic, offline failure sequence.
        services.AddHttpClient("PollyRetryClient")
            .ConfigurePrimaryHttpMessageHandler(() => new FailNTimesHandler(failureCount: 2))
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(20 * retryAttempt)));

        // Microsoft.Extensions.Http.Resilience (Polly v8), the successor to the policy above,
        // against the same kind of deterministic sequence.
        services.AddHttpClient("StandardResilienceClient")
            .ConfigurePrimaryHttpMessageHandler(() => new FailNTimesHandler(failureCount: 2))
            .AddStandardResilienceHandler();

        services.AddHttpClient("FhirHttpClient")
            .AddHttpMessageHandler<LoggingHandler>()
            .AddHttpMessageHandler<FhirHeadersHandler>();

        services.AddSingleton<IFhirService, FhirService>();
    }
}
