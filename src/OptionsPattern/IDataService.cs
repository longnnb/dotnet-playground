namespace OptionsPattern;

/// <summary>An external configuration source that has nothing to do with <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> -- a stand-in for a database or remote settings service.</summary>
public interface IDataService
{
    /// <summary>Synchronously fetches a connection string.</summary>
    string GetConnectionString();

    /// <summary>Asynchronously fetches a connection string. See <see cref="Demos.AsyncConfigurationDemo"/> for why nothing in the options pipeline itself can call this.</summary>
    Task<string> GetConnectionStringAsync();

    /// <summary>Synchronously fetches a timeout.</summary>
    int GetTimeout();

    /// <summary>Asynchronously fetches a timeout. See <see cref="Demos.AsyncConfigurationDemo"/> for why nothing in the options pipeline itself can call this.</summary>
    Task<int> GetTimeoutAsync();
}
