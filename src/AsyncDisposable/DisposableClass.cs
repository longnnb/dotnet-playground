namespace AsyncDisposable;
internal class DisposableClass(string name = "") : IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        Console.WriteLine($"Dispose called from class {name}");
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(100);
        Console.WriteLine($"DisposeAsync called from class {name}");
    }
}