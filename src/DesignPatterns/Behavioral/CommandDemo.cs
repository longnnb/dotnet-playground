namespace DesignPatterns.Behavioral;

/// <summary>
/// Command: turns a request into a standalone object carrying the state needed to both perform
/// and reverse it -- which is what makes undo/redo possible, the feature that actually
/// justifies this pattern's ceremony.
/// </summary>
public static class CommandDemo
{
    /// <summary>Runs the demo: apply commands, undo, redo, and print the buffer after each step.</summary>
    public static Task RunAsync()
    {
        var document = new TextDocument();
        var history = new CommandHistory();

        history.Execute(new AppendTextCommand(document, "Hello"));
        history.Execute(new AppendTextCommand(document, ", world"));
        history.Execute(new AppendTextCommand(document, "!"));
        Console.WriteLine($"After 3 commands: \"{document.Text}\"");

        history.Undo();
        Console.WriteLine($"After 1 undo:     \"{document.Text}\"");

        history.Undo();
        Console.WriteLine($"After 2 undos:    \"{document.Text}\"");

        history.Redo();
        Console.WriteLine($"After 1 redo:     \"{document.Text}\"");

        return Task.CompletedTask;
    }
}

file sealed class TextDocument
{
    public string Text { get; private set; } = string.Empty;

    public void Append(string text) => Text += text;

    public void RemoveFromEnd(int length) => Text = Text[..^length];
}

file interface ICommand
{
    void Execute();
    void Undo();
}

file sealed class AppendTextCommand(TextDocument document, string text) : ICommand
{
    public void Execute() => document.Append(text);

    public void Undo() => document.RemoveFromEnd(text.Length);
}

file sealed class CommandHistory
{
    private readonly Stack<ICommand> _done = new();
    private readonly Stack<ICommand> _undone = new();

    public void Execute(ICommand command)
    {
        command.Execute();
        _done.Push(command);
        _undone.Clear();
    }

    public void Undo()
    {
        if (_done.Count == 0)
        {
            return;
        }

        var command = _done.Pop();
        command.Undo();
        _undone.Push(command);
    }

    public void Redo()
    {
        if (_undone.Count == 0)
        {
            return;
        }

        var command = _undone.Pop();
        command.Execute();
        _done.Push(command);
    }
}

/* Expected output
After 3 commands: "Hello, world!"
After 1 undo:     "Hello, world"
After 2 undos:    "Hello"
After 1 redo:     "Hello, world"
*/
