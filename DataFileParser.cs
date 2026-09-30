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

        // Pass 1: turn every text line into (depth, type, text)
        List<ParsedLine> parsedLines = ParseLines(lines);

        // Pass 2: build the tree recursively.
        // Virtual root with no text, so the file may contain more than one top-level direction
        Node root = new Node(string.Empty);
        Dictionary<string, Node> items = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
        int index = 0;

        // Attaches all consecutive lines at 'depth' to 'parent'.
        // A direction recurses one level deeper for its own children.
        // The first shallower line closes this branch and returns control to the caller.
        void ParseChildren(Node parent, int depth)
        {
            while (index < parsedLines.Count)
            {
                ParsedLine line = parsedLines[index];

                // Branch is finished, the line belongs to an ancestor
                if (line.Depth < depth)
                    return;

                // A line may be at most one level deeper than its parent
                if (line.Depth > depth)
                    throw new DataFileException("Unexpected indentation (a level was skipped or an item has children).", line.LineNumber);

                index++;

                Node node = new Node(line.Text, line.IsItem);
                parent.AddChild(node);

                if (line.IsItem)
                {
                    if (!items.TryAdd(line.Text, node))
                        throw new DataFileException($"Item '{line.Text}' is defined more than once.", line.LineNumber);
                }
                else
                {
                    // Open a new branch; following deeper lines become its children
                    ParseChildren(node, depth + 1);
                }
            }
        }

        ParseChildren(root, 0);

        if (items.Count == 0)
            throw new DataFileException("Data file does not contain any items.");

        return new ParseResult(root, items);
    }

    /// <summary>
    /// Validates and converts every raw line into a <see cref="ParsedLine"/>.
    /// </summary>
    private static List<ParsedLine> ParseLines(IReadOnlyList<string> lines)
    {
        List<ParsedLine> result = new List<ParsedLine>(lines.Count);

        for (int i = 0; i < lines.Count; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i].TrimEnd();

            if (line.Length == 0)
                throw new DataFileException("Blank lines are not allowed.", lineNumber);

            (int depth, bool isItem, string text) = ParseLine(line, lineNumber);
            result.Add(new ParsedLine(depth, isItem, text, lineNumber));
        }

        return result;
    }

    /// <summary>
    /// One line of the data file after it has been split into its parts.
    /// </summary>
    private record ParsedLine(int Depth, bool IsItem, string Text, int LineNumber);

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
