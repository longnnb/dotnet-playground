// See https://aka.ms/new-console-template for more information

using AsyncDisposable;

using (var dc = new DisposableClass("1"))
{
    Console.WriteLine("Using block 1");
}

await using (var dc = new DisposableClass("2"))
{
    Console.WriteLine("Await using block 2");
}

Console.WriteLine(await TestUsingInMethod());

static async Task<string> TestUsingInMethod()
{
    try
    {
        await using var dc = new DisposableClass("3");
        Console.WriteLine("Await using block in test method 3");
        //throw new Exception("Test Error message");
        return "Normal result";
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return "Exception result";
    }
    finally
    {
        Console.WriteLine("Finally block");
    }
}

/*
 * Output:
Using block 1
Dispose called from class 1
Await using block 2
DisposeAsync called from class 2
Await using block in test method 3
DisposeAsync called from class 3
Finally block
Normal result
 */

/* 
 * Error output
Using block 1
Dispose called from class 1
Await using block 2
DisposeAsync called from class 2
Await using block in test method 3
DisposeAsync called from class 3
Test Error message
Finally block
Exception result
 */