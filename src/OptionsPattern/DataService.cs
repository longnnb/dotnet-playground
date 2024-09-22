using System.Globalization;

namespace TestOption;

public class DataService : IDataService
{
    public string GetConnectionString()
    {
        // Simulate fetching the connection string from a data service
        return "ConnectionString From Data Service";
    }

    public async Task<string> GetConnectionStringAsync()
    {
        await Task.Delay(1000);
        return "ConnectionString From Data Service async";
    }

    public int GetTimeout()
    {
        // Simulate fetching the timeout value from a data service
        return 45;
    }

    public async Task<int> GetTimeoutAsync()
    {
        await Task.Delay(1000);
        return 50;
    }
}