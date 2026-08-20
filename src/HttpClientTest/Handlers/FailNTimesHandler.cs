using System.Net;

namespace HttpClientTest.Handlers;

/// <summary>
/// A primary (terminal) <see cref="DelegatingHandler"/> that returns
/// <see cref="HttpStatusCode.ServiceUnavailable"/> for the first <paramref name="failureCount"/>
/// requests it sees, then <see cref="HttpStatusCode.OK"/> from then on. Makes retry/resilience
/// demos deterministic and fully offline, instead of depending on a remote server actually
/// failing at the right moment.
/// </summary>
public class FailNTimesHandler(int failureCount) : DelegatingHandler
{
    private int _attempts;

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var attempt = Interlocked.Increment(ref _attempts);

        var response = attempt <= failureCount
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"status\":\"ok\"}") };

        response.RequestMessage = request;
        return Task.FromResult(response);
    }
}
