// See https://aka.ms/new-console-template for more information

using AsyncDisposable;

using (var dc = new DisposableClass())
{
    Console.WriteLine("Using block");
}

await using (var dc = new DisposableClass())
{
    Console.WriteLine("Await using block");
}

