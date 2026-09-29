namespace Eaton;

/// <summary>
/// A single node of the n-ary tree parsed from the data file.
/// A node is either a direction (inner node) or an item (leaf).
/// </summary>
public class Node
{
    public string Text { get; private set; } = string.Empty;
    
    public bool IsItem { get; private set; }

    public Node? Parent { get; private set; }

    public List<Node> Children { get; } = new();

    public Node(string text, bool isItem = false)
    {
        Text = text;
        IsItem = isItem;
    }

    /// <summary>
    /// Adds a child node and sets this node as its parent.
    /// </summary>
    public void AddChild(Node child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    /// <summary>
    /// Returns the directions from the root down to this node's parent.
    /// Nodes with empty text (e.g. a virtual root) are skipped.
    /// </summary>
    public List<string> GetDirections()
    {
        var directions = new List<string>();

        // Walk up the tree towards the root
        for (var current = Parent; current != null; current = current.Parent)
        {
            if (!string.IsNullOrEmpty(current.Text))
                directions.Add(current.Text);
        }

        // Collected from leaf to root, so flip it
        directions.Reverse();
        return directions;
    }

    public override string ToString() => IsItem ? $"Item: {Text}" : Text;
}
