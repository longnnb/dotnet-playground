namespace HttpClientTest;

public interface IMyService
{
    Task<string> GetDataAsync();
}

public class MyService : IMyService
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

        return string.Empty;
    }
}