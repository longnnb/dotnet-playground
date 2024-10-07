using System.Text.Json;
using Hl7.Fhir.Model;

namespace HttpClientTest;

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log the request details
        // Console.WriteLine($"Request: {request.Method} {request.RequestUri}");
        // Console.WriteLine($"Request:");
        // Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(request));
        // Call the inner handler to send the request to the server
        var httpResponse = await base.SendAsync(request, cancellationToken);

        // Log the response details
        // Console.WriteLine($"Response:");
        // Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(httpResponse));
        
        // if (httpResponse.Content != null)
        // {
        //     var responseContent = JsonSerializer.Deserialize<Resource>(await httpResponse.Content.ReadAsStringAsync());
        //     Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(responseContent));
        // }

        return httpResponse;
    }
}