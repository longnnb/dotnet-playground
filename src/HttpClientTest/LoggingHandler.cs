using System.Text.Json;
using Hl7.Fhir.Model;

namespace HttpClientTest;

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Request: {request.Method} {request.RequestUri}");
        if (request.Content != null)
            Console.WriteLine(await request.Content.ReadAsStringAsync());
        try
        {
            var httpResponse = await base.SendAsync(request, cancellationToken);
            // Console.WriteLine($"Response: {await httpResponse.Content.ReadAsStringAsync()}");
            return httpResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine("From loggin hander: " + e.Message);
            throw;
        }
    }
}