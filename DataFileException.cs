namespace Eaton;

/// <summary>
/// Thrown when the data file is missing or does not match the expected format.
/// </summary>
public class DataFileException : Exception
{
    /// <summary>
    /// 1-based line number where the problem was found, or null if it is not line specific.
    /// </summary>
    public int? LineNumber { get; }

    public DataFileException(string message, int? lineNumber = null, Exception? inner = null)
        : base(lineNumber.HasValue ? $"Line {lineNumber}: {message}" : message, inner)
    {
        LineNumber = lineNumber;
    }
}
