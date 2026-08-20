namespace HttpClientTest;

/// <summary>Configuration for <see cref="Handlers.ProxyHttpClientHandler"/>, bound from the <c>Proxy</c> section of <c>appsettings.json</c>.</summary>
public class ProxyOptions
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Proxy";

    /// <summary>
    /// Whether the typed client demo routes through <see cref="Handlers.ProxyHttpClientHandler"/>.
    /// Defaults to <see langword="false"/>: <see cref="Address"/> below is a placeholder that
    /// doesn't resolve to anything, so leaving this on would make every request through the
    /// typed client fail. An earlier version of this project wired the proxy handler in
    /// unconditionally with nothing registering it in DI at all, which crashed the moment
    /// anything actually resolved the typed client.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>The proxy address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>The proxy username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>The proxy password.</summary>
    public string Password { get; set; } = string.Empty;
}
