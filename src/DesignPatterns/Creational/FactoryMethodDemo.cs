namespace DesignPatterns.Creational;

/// <summary>
/// Factory Method: a creation method picks and returns the right concrete type at runtime, so
/// the caller never names a concrete type itself -- here, a document-parser factory that picks
/// a parser by file extension.
/// </summary>
/// <remarks>
/// In modern .NET, most of what Factory Method was for is subsumed by DI: registering an
/// interface with multiple implementations and letting <see cref="IServiceProvider"/> or
/// <c>ActivatorUtilities.CreateInstance</c> pick and construct the right one. Reach for a plain
/// Factory Method when the construction logic is simple, has no external dependencies, and
/// doesn't need a container -- see the <c>DependencyInjection</c> project for the
/// container-based answer.
/// </remarks>
public static class FactoryMethodDemo
{
    /// <summary>Runs the demo over a few file extensions.</summary>
    public static Task RunAsync()
    {
        foreach (var path in new[] { "report.csv", "data.json", "notes.txt" })
        {
            var parser = DocumentParserFactory.CreateParser(path);
            Console.WriteLine($"{path} -> {parser.Name}: {parser.Parse(path)}");
        }

        return Task.CompletedTask;
    }
}

file interface IDocumentParser
{
    // Deliberately not using GetType().Name to identify the concrete parser at the call site:
    // `file`-scoped types get a compiler-mangled runtime name, which would leak into output
    // that's supposed to be a clean demonstration of which type the factory picked.
    string Name { get; }

    string Parse(string path);
}

file sealed class CsvParser : IDocumentParser
{
    public string Name => nameof(CsvParser);

    public string Parse(string path) => $"parsed '{path}' as comma-separated rows";
}

file sealed class JsonParser : IDocumentParser
{
    public string Name => nameof(JsonParser);

    public string Parse(string path) => $"parsed '{path}' as a JSON document";
}

file sealed class PlainTextParser : IDocumentParser
{
    public string Name => nameof(PlainTextParser);

    public string Parse(string path) => $"parsed '{path}' as plain text";
}

file static class DocumentParserFactory
{
    public static IDocumentParser CreateParser(string path) => Path.GetExtension(path) switch
    {
        ".csv" => new CsvParser(),
        ".json" => new JsonParser(),
        _ => new PlainTextParser(),
    };
}

/* Expected output
report.csv -> CsvParser: parsed 'report.csv' as comma-separated rows
data.json -> JsonParser: parsed 'data.json' as a JSON document
notes.txt -> PlainTextParser: parsed 'notes.txt' as plain text
*/
