using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDisposable;
internal class DisposableClass : IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        Console.WriteLine("Dispose called");
    }

    public async ValueTask DisposeAsync()
    {
        //await Task.Delay(1000);
        Console.WriteLine("DisposeAsync called");
    }
}