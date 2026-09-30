namespace Eaton;

/// <summary>
/// Finds all data files (*.txt) in a folder and lets the user pick one with a single key press.
/// </summary>
public class DataFileSelector
{
    private readonly string _folder;

    public DataFileSelector(string folder)
    {
        _folder = folder;
    }

    /// <summary>
    /// Returns all *.txt files in the folder, sorted by file name.
    /// </summary>
    private IReadOnlyList<string> FindDataFiles()
    {
        if (!Directory.Exists(_folder))
            throw new DataFileException($"Data folder '{_folder}' was not found.");

        var files = Directory.GetFiles(_folder, "*.txt")
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
            throw new DataFileException($"No data files (*.txt) were found in '{_folder}'.");

        return files;
    }

    /// <summary>
    /// Shows the available data files and waits until the user presses a valid key.
    /// Returns the full path of the selected file.
    /// </summary>
    public string SelectFile()
    {
        IReadOnlyList<string> files = FindDataFiles();

        // Nothing to choose from, so don't bother the user
        if (files.Count == 1)
        {
            Console.WriteLine($"Using the only data file found: {Path.GetFileName(files[0])}");
            return files[0];
        }

        var selectable = files.Take(SelectionKeys.MaxCount).ToList();

        Console.WriteLine("Available data files:");
        for (var i = 0; i < selectable.Count; i++)
            Console.WriteLine($"[{SelectionKeys.GetKey(i)}] - {Path.GetFileName(selectable[i])}");

        if (files.Count > selectable.Count)
            Console.WriteLine($"(Only the first {selectable.Count} of {files.Count} files can be selected.)");

        Console.WriteLine($"Which data file would you like to load? ({SelectionKeys.DescribeRange(selectable.Count)})");

        while (true)
        {
            // intercept: true -> the key is not echoed, so invalid keys leave no trace
            var key = Console.ReadKey(intercept: true);

            if (SelectionKeys.TryGetIndex(key.KeyChar, selectable.Count, out int index))
            {
                Console.WriteLine(SelectionKeys.GetKey(index));
                return selectable[index];
            }

            // Any other key is ignored and we keep waiting
        }
    }
}
