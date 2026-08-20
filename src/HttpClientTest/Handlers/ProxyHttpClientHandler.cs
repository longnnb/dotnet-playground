using System.Net;
using Microsoft.Extensions.Options;

namespace HttpClientTest.Handlers;

/// <summary>
/// Routes requests through an HTTP proxy configured via <see cref="ProxyOptions"/>. Only wired
/// into the typed client when <see cref="ProxyOptions.Enabled"/> is <see langword="true"/> --
/// see <c>Program.ConfigureServices</c> -- because <see cref="ProxyOptions.Address"/>'s default
/// value doesn't resolve to a real proxy.
/// </summary>
public class ProxyHttpClientHandler : HttpClientHandler
{
    /// <summary>Configures the proxy from the given <paramref name="options"/>.</summary>
    public ProxyHttpClientHandler(IOptions<ProxyOptions> options)
    {
        var proxyOptions = options.Value;

        Proxy = new WebProxy
        {
            Address = new Uri(proxyOptions.Address),
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(proxyOptions.Username, proxyOptions.Password),
        };

        UseProxy = true;
    }
}
