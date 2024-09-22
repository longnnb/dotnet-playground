namespace TestOption;

public interface IDataService
{
    string GetConnectionString();
    Task<string> GetConnectionStringAsync();
    int GetTimeout();
    Task<int> GetTimeoutAsync();
}