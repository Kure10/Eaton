using System.Text;
using Eaton;

// Tree connectors ("├──") need UTF-8 to be printed correctly on Windows consoles
Console.OutputEncoding = Encoding.UTF8;

string dataFilePath;
ParseResult data;

try
{
    // A data file can be passed as the first argument, otherwise the user picks one from TestData
    dataFilePath = args.Length > 0
        ? args[0]
        : new DataFileSelector(Path.Combine(AppContext.BaseDirectory, "TestData")).SelectFile();

    data = new DataFileParser().Parse(dataFilePath);
}
catch (DataFileException ex)
{
    Console.WriteLine($"FAILURE: Could not load the data file. {ex.Message}");
    return 1;
}

Console.WriteLine();
Console.WriteLine($"Loaded '{Path.GetFileName(dataFilePath)}' with {data.Items.Count} items.");
Console.WriteLine();
PrintTree(data.Root, 0);

return 0;

// Debug helper: prints the parsed tree with indentation
static void PrintTree(Node node, int depth)
{
    foreach (var child in node.Children)
    {
        Console.WriteLine($"{new string(' ', depth * 2)}{child}");
        PrintTree(child, depth + 1);
    }
}
