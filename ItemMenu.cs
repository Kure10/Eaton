namespace Eaton;

/// <summary>
/// Interactive menu: lists all items in reverse alphabetical order,
/// lets the user pick one with a single key press and prints the directions to it.
/// Repeats until the user presses Esc.
/// </summary>
public class ItemMenu
{
    private readonly List<Node> _items;
    private readonly int _totalItemCount;

    public ItemMenu(IReadOnlyDictionary<string, Node> items)
    {
        _totalItemCount = items.Count;

        // Reverse alphabetical order, as required by the assignment
        _items = items.Values
            .OrderByDescending(item => item.Text, StringComparer.OrdinalIgnoreCase)
            .Take(SelectionKeys.MaxCount)
            .ToList();
    }

    public void Run()
    {
        while (true)
        {
            PrintItems();

            Node? selected = ReadSelection();
            if (selected == null)
                return;

            PrintDirections(selected);
        }
    }

    private void PrintItems()
    {
        Console.WriteLine();
        Console.WriteLine("Available items:");

        for (int i = 0; i < _items.Count; i++)
            Console.WriteLine($"[{SelectionKeys.GetKey(i)}] - {_items[i].Text}");

        if (_totalItemCount > _items.Count)
            Console.WriteLine($"(Only the first {_items.Count} of {_totalItemCount} items can be selected.)");

        Console.WriteLine("What item would you like to search for? (Esc to exit)");
    }

    /// <summary>
    /// Waits for a valid key. Returns the selected item, or null when the user pressed Esc.
    /// </summary>
    private Node? ReadSelection()
    {
        while (true)
        {
            // Collect the desired item of the user
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Escape)
                return null;

            if (SelectionKeys.TryGetIndex(key.KeyChar, _items.Count, out int index))
            {
                Console.WriteLine(SelectionKeys.GetKey(index));
                return _items[index];
            }

            // Any other key is ignored and we keep waiting
        }
    }

    private static void PrintDirections(Node item)
    {
        foreach (string direction in item.GetDirections())
            Console.WriteLine(direction);
    }
}
