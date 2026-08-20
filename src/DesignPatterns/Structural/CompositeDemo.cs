namespace DesignPatterns.Structural;

/// <summary>
/// Composite: lets client code treat a single object and a collection of objects through the
/// same interface, so operations recurse naturally over a tree -- here, computing the total
/// size of a directory tree without the caller ever branching on "is this a file or a folder?".
/// </summary>
public static class CompositeDemo
{
    /// <summary>Runs the demo over a small hand-built directory tree.</summary>
    public static Task RunAsync()
    {
        var root = new DirectoryNode("project",
        [
            new FileNode("README.md", 2_048),
            new DirectoryNode("src",
            [
                new FileNode("Program.cs", 1_200),
                new FileNode("Helpers.cs", 3_400),
            ]),
            new DirectoryNode("docs",
            [
                new FileNode("guide.md", 5_000),
            ]),
        ]);

        Print(root, indent: 0);
        Console.WriteLine($"Total size: {root.Size} bytes");

        return Task.CompletedTask;

        // Local function so the file-local IFileSystemNode/DirectoryNode types it references
        // never need to appear in a member signature of the public CompositeDemo class itself.
        static void Print(IFileSystemNode node, int indent)
        {
            Console.WriteLine($"{new string(' ', indent * 2)}{node.Name} ({node.Size} bytes)");

            if (node is DirectoryNode directory)
            {
                foreach (var child in directory.Children)
                {
                    Print(child, indent + 1);
                }
            }
        }
    }
}

file interface IFileSystemNode
{
    string Name { get; }
    long Size { get; }
}

file sealed class FileNode(string name, long size) : IFileSystemNode
{
    public string Name { get; } = name;
    public long Size { get; } = size;
}

file sealed class DirectoryNode(string name, IReadOnlyList<IFileSystemNode> children) : IFileSystemNode
{
    public string Name { get; } = name;
    public IReadOnlyList<IFileSystemNode> Children { get; } = children;
    public long Size => Children.Sum(c => c.Size);
}

/* Expected output
project (11648 bytes)
  README.md (2048 bytes)
  src (4600 bytes)
    Program.cs (1200 bytes)
    Helpers.cs (3400 bytes)
  docs (5000 bytes)
    guide.md (5000 bytes)
Total size: 11648 bytes
*/
