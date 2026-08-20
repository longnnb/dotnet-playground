namespace DesignPatterns.Creational;

/// <summary>
/// Builder: assembles a complex object step by step through a fluent interface, deferring
/// construction of the final immutable result to a single <c>Build()</c> call.
/// </summary>
/// <remarks>
/// .NET has real builders that follow this exact shape: <see cref="System.Data.Common.DbConnectionStringBuilder"/>
/// for connection strings, and <c>Host.CreateApplicationBuilder()</c> -- used throughout this
/// repo's DI-based projects -- for composing an application's services and configuration.
/// </remarks>
public static class BuilderDemo
{
    /// <summary>Runs the demo, assembling one request via the fluent builder.</summary>
    public static Task RunAsync()
    {
        var request = new HttpRequestBuilder()
            .WithMethod("POST")
            .WithUrl("https://example.test/api/widgets")
            .WithHeader("Accept", "application/json")
            .WithHeader("X-Trace-Id", "abc-123")
            .WithBody("{\"name\":\"widget\"}")
            .Build();

        Console.WriteLine(request);

        return Task.CompletedTask;
    }
}

file sealed record BuiltRequest(string Method, string Url, IReadOnlyDictionary<string, string> Headers, string? Body)
{
    public override string ToString()
    {
        var headerText = string.Join(", ", Headers.Select(h => $"{h.Key}={h.Value}"));
        return $"{Method} {Url}  headers=[{headerText}]  body={Body ?? "(none)"}";
    }
}

file sealed class HttpRequestBuilder
{
    private string _method = "GET";
    private string _url = string.Empty;
    private readonly Dictionary<string, string> _headers = [];
    private string? _body;

    public HttpRequestBuilder WithMethod(string method)
    {
        _method = method;
        return this;
    }

    public HttpRequestBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public HttpRequestBuilder WithHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }

    public HttpRequestBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    public BuiltRequest Build() => new(_method, _url, _headers, _body);
}

/* Expected output
POST https://example.test/api/widgets  headers=[Accept=application/json, X-Trace-Id=abc-123]  body={"name":"widget"}
*/
