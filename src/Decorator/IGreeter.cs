namespace Decorator;

/// <summary>A trivial greeting service, wrapped in various ways by the demos in this project.</summary>
public interface IGreeter
{
    /// <summary>Returns a greeting for <paramref name="name"/>.</summary>
    string Greet(string name);
}
