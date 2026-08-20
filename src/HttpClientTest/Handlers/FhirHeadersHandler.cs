namespace HttpClientTest.Handlers;

/// <summary>
/// Adds a per-request trace id header to every FHIR request. An earlier version of
/// <see cref="FhirService"/> set this (and a second, operation-name header) by hand in all four
/// of its methods; a <see cref="DelegatingHandler"/> is the mechanism this project is actually
/// about, and it's the right place for a cross-cutting request header like this one.
/// </summary>
public class FhirHeadersHandler : DelegatingHandler
{
    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("x-request-id", Guid.NewGuid().ToString());

        return base.SendAsync(request, cancellationToken);
    }
}
