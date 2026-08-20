namespace OptionsPattern;

/// <summary>
/// Stands in for a real external configuration source (a database, a remote config service).
/// The sync and async members deliberately return different values so a demo can tell which
/// path actually ran -- see <see cref="Demos.AsyncConfigurationDemo"/> for why the async pair
/// exists but was never called from the options pipeline itself.
/// </summary>
public class DataService : IDataService
{
    /// <inheritdoc />
    public string GetConnectionString()
    {
        // Simulate fetching the connection string from a data service.
        return "ConnectionString from DataService (sync)";
    }

    /// <inheritdoc />
    public async Task<string> GetConnectionStringAsync()
    {
        await Task.Delay(200);
        return "ConnectionString from DataService (async)";
    }

    /// <inheritdoc />
    public int GetTimeout()
    {
        // Simulate fetching the timeout value from a data service.
        return 45;
    }

    /// <inheritdoc />
    public async Task<int> GetTimeoutAsync()
    {
        await Task.Delay(200);
        return 50;
    }
}
