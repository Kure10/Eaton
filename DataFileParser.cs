namespace Eaton;

/// <summary>
/// Result of parsing a data file: the tree itself plus a lookup of all items by name.
/// </summary>
public record ParseResult(Node Root, IReadOnlyDictionary<string, Node> Items);

/// <summary>
/// Parses the data file into an n-ary tree of directions and items.
///
/// Each line looks like: [prefix][connector][marker][text]
///   prefix    - "|  " or "   " blocks, one per ancestor level
///   connector - "├──" or "└──" (missing on the root line)
///   marker    - "+ " for a direction, " Item: " for an item
///
/// The depth of a line is derived from the position of its connector.
/// </summary>
public class DataFileParser
{
    private const int IndentSize = 3;
    private const string DirectionMarker = "+ ";
    private const string ItemMarker = "Item: ";

    public ParseResult Parse(string path)
    {
        if (!File.Exists(path))
            throw new DataFileException($"Data file '{path}' was not found.");

        string[] lines;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new DataFileException($"Data file '{path}' could not be read.", inner: ex);
        }

        return Parse(lines);
    }

    private ParseResult Parse(IReadOnlyList<string> lines)
    {
        if (lines.Count == 0)
            throw new DataFileException("Data file is empty.");

        // Virtual root with no text, so the file may contain more than one top-level direction
        var root = new Node(string.Empty);
        var items = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);

        // Stack of currently open directions; the bottom is always the virtual root.
        // Invariant: stack.Count == depth of the next child + 1
        Stack<Node> stack = new Stack<Node>();
        stack.Push(root);

        for (int i = 0; i < lines.Count; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i].TrimEnd();

            if (line.Length == 0)
                throw new DataFileException("Blank lines are not allowed.", lineNumber);

            (int depth, bool isItem, string text) = ParseLine(line, lineNumber);

            // Close every branch that is deeper than (or at the same level as) this line
            while (stack.Count > depth + 1)
                stack.Pop();

            // A line may be at most one level deeper than its parent
            if (stack.Count < depth + 1)
                throw new DataFileException("Unexpected indentation (a level was skipped or an item has children).", lineNumber);

            Node node = new Node(text, isItem);
            stack.Peek().AddChild(node);

            if (isItem)
            {
                if (!items.TryAdd(text, node))
                    throw new DataFileException($"Item '{text}' is defined more than once.", lineNumber);
            }
            else
            {
                // Open a new branch; following deeper lines become its children
                stack.Push(node);
            }
        }

        if (items.Count == 0)
            throw new DataFileException("Data file does not contain any items.");

        return new ParseResult(root, items);
    }
    
    /// <summary>
    /// Splits a single line into its depth, type and text.
    /// </summary>
    private static (int Depth, bool IsItem, string Text) ParseLine(string line, int lineNumber)
    {
        var depth = 0;
        var contentStart = 0;

        // Find the connector ("├──" or "└──"); the root line has none
        int connectorIndex = line.IndexOfAny(['├', '└']);
        if (connectorIndex >= 0)
        {
            if (connectorIndex % IndentSize != 0)
                throw new DataFileException("Connector is not aligned to the indentation grid.", lineNumber);

            // Everything in front of the connector must be indentation only
            if (line[..connectorIndex].Any(c => c != ' ' && c != '|'))
                throw new DataFileException("Unexpected characters in the indentation.", lineNumber);

            if (!line.AsSpan(connectorIndex + 1).StartsWith("──"))
                throw new DataFileException("Connector must be followed by '──'.", lineNumber);

            depth = connectorIndex / IndentSize + 1;
            contentStart = connectorIndex + IndentSize;
        }

        var content = line[contentStart..];

        if (content.StartsWith(DirectionMarker))
            return (depth, false, RequireText(content[DirectionMarker.Length..], lineNumber));

        // Items are written as "└── Item: Name", so skip the separating space
        var trimmed = content.TrimStart();
        if (trimmed.StartsWith(ItemMarker))
            return (depth, true, RequireText(trimmed[ItemMarker.Length..], lineNumber));

        throw new DataFileException("Line is neither a direction ('+ ') nor an item ('Item: ').", lineNumber);
    }

    private static string RequireText(string text, int lineNumber)
    {
        text = text.Trim();
        if (text.Length == 0)
            throw new DataFileException("Direction or item text is empty.", lineNumber);
        return text;
    }
}
