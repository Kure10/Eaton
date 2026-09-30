namespace Eaton;

/// <summary>
/// Maps list positions to single keys: 1-9 first, then a-z.
/// This lets the user pick from up to 35 entries with a single key press.
/// </summary>
public static class SelectionKeys
{
    private const string Keys = "123456789abcdefghijklmnopqrstuvwxyz";

    public static int MaxCount => Keys.Length;

    /// <summary>
    /// Returns the key shown for the entry at the given 0-based index.
    /// </summary>
    public static char GetKey(int index) => Keys[index];

    /// <summary>
    /// Translates a pressed key back to a 0-based index.
    /// Letters are case-insensitive. Returns false for keys outside the first 'count' entries.
    /// </summary>
    public static bool TryGetIndex(char keyChar, int count, out int index)
    {
        index = Keys.IndexOf(char.ToLowerInvariant(keyChar));
        return index >= 0 && index < count;
    }

    /// <summary>
    /// Human readable range of valid keys, e.g. "1-9" or "1-9, a-c".
    /// </summary>
    public static string DescribeRange(int count)
    {
        if (count <= 9)
            return $"1-{count}";

        return $"1-9, a-{GetKey(count - 1)}";
    }
}
