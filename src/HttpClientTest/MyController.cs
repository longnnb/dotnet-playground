namespace HttpClientTest;

public class MyController
{
    private readonly IHttpClientFactory _clientFactory;

    public MyController(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    
    public async Task<string> GetData()
    {
        var client = _clientFactory.CreateClient("MyClient");
        var response = await client.GetAsync("/data");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        return string.Empty;
    }
}