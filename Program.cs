using System.Text;
using Eaton;

// Tree connectors ("├──") need UTF-8 to be printed correctly on Windows consoles
Console.OutputEncoding = Encoding.UTF8;

try
{
    // Where the data comes from, in order of priority:
    //   1. a path passed as the first argument
    //   2. "Data.txt" next to the executable or in the current folder (as named in the assignment)
    //   3. the user picks one of the sample files in TestData
    string dataFilePath = args.Length > 0
        ? args[0]
        : FindDefaultDataFile()
          ?? new DataFileSelector(Path.Combine(AppContext.BaseDirectory, "TestData")).SelectFile();

    ParseResult data = new DataFileParser().Parse(dataFilePath);

    new ItemMenu(data.Items).Run();
}
catch (DataFileException ex)
{
    Console.WriteLine($"FAILURE: Could not load the data file. {ex.Message}");
    return 1;
}
catch (InvalidOperationException ex) when (Console.IsInputRedirected)
{
    // Console.ReadKey does not work when input is piped from a file
    Console.WriteLine($"FAILURE: This application needs an interactive console. {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"FAILURE: An unexpected error occurred. {ex.Message}");
    return 1;
}

return 0;

// Returns the full path of "Data.txt" if it exists next to the executable or in the current folder, otherwise null
static string? FindDefaultDataFile()
{
    const string defaultFileName = "Data.txt";

    string[] candidates =
    [
        Path.Combine(AppContext.BaseDirectory, defaultFileName),
        Path.Combine(Directory.GetCurrentDirectory(), defaultFileName)
    ];

    string? found = candidates.FirstOrDefault(File.Exists);
    if (found != null)
        Console.WriteLine($"Using {defaultFileName} found in '{Path.GetDirectoryName(found)}'.");

    return found;
}
