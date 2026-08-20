namespace HttpClientTest;

/// <summary>
/// A typed <see cref="HttpClient"/> wrapper. <see cref="HttpClient.BaseAddress"/> is configured
/// in the <c>AddHttpClient&lt;ITodoClient, TodoClient&gt;()</c> registration lambda (see
/// <c>Program.ConfigureServices</c>), not here in the constructor -- an earlier version of this
/// class set it in the constructor instead, which is the wrong place for a typed client:
/// configuration belongs with the registration, not scattered into the implementation, and it
/// meant the same base-address literal was duplicated between here and the named-client demo.
/// </summary>
public class TodoClient(HttpClient httpClient) : ITodoClient
{
    /// <inheritdoc />
    public async Task<string> GetTodoAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/todos/{id}", cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;
    }
}
