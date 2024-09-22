namespace HttpClientTest;

public class MyService
{
    private readonly HttpClient _httpClient;

    public MyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.example.com");
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("/data");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return null;
    }
}