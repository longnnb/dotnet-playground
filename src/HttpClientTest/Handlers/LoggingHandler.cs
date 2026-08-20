using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HttpClientTest.Handlers;

/// <summary>
/// Logs each request/response pair with its elapsed time, via an injected
/// <see cref="ILogger{TCategoryName}"/> rather than raw <see cref="Console.WriteLine(string)"/>
/// -- the project already has full DI/logging available, so there's no reason not to use it.
/// Registered on multiple named/typed clients (see <c>Program.ConfigureServices</c>), showing
/// that one handler <em>type</em> can sit in front of several different pipelines; each client
/// gets its own handler <em>instance</em>, since <c>AddHttpMessageHandler&lt;LoggingHandler&gt;()</c>
/// resolves a fresh one per client.
/// </summary>
public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation("Request: {Method} {Uri}", request.Method, request.RequestUri);

        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "Response: {StatusCode} for {Method} {Uri} in {ElapsedMs}ms",
                (int)response.StatusCode, request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex, "Request failed: {Method} {Uri} after {ElapsedMs}ms",
                request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
