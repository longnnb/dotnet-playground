namespace AsyncDisposable;

internal class DisposableClass(string name = "") : IDisposable, IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Task.Delay(100);
        Console.WriteLine($"DisposeAsync called from class {name}");
    }

    public void Dispose()
    {
        Console.WriteLine($"Dispose called from class {name}");
    }
}