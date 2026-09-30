using System.Text;
using Eaton;

// Tree connectors ("├──") need UTF-8 to be printed correctly on Windows consoles
Console.OutputEncoding = Encoding.UTF8;

try
{
    // A data file can be passed as the first argument, otherwise the user picks one from TestData
    string dataFilePath = args.Length > 0
        ? args[0]
        : new DataFileSelector(Path.Combine(AppContext.BaseDirectory, "TestData")).SelectFile();

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
