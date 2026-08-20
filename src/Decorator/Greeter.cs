namespace Decorator;

/// <summary>The undecorated implementation of <see cref="IGreeter"/>.</summary>
public class Greeter : IGreeter
{
    /// <inheritdoc />
    public string Greet(string name) => $"Hello, {name}!";
}
