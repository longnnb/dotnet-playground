namespace HttpClientTest;

/// <summary>A typed client wrapping a real public test API, demonstrating the typed-client pattern.</summary>
public interface ITodoClient
{
    /// <summary>Fetches a single to-do item by id.</summary>
    Task<string> GetTodoAsync(int id, CancellationToken cancellationToken = default);
}
