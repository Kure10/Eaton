# Item Directions

A .NET console application that reads a data file with directions laid out as a tree,
lets the user pick an item with a single key press and prints the directions to that item.

## Requirements

- .NET 10 SDK

## How to run

```bash
dotnet run
```

Or open `Eaton.sln` in Rider / Visual Studio and run the project.

### Where the data comes from

The application looks for the data file in this order:

1. **Command-line argument**: `dotnet run -- path/to/file.txt`
2. **`Data.txt`** next to the executable or in the current folder
   (a `Data.txt` placed in the project folder is copied to the output on build)
3. **Sample files** in the `TestData` folder: the user picks one from a menu

## Example

```
Available items:
[1] - Pencils
[2] - Mobile Phone
[3] - Milk
[4] - Cookies
[5] - Coffee Mug
What item would you like to search for? (Esc to exit)
2
Walk to the end of the hall.
Turn right.
Go through the door at the end of the hall.
Look on top of the desk.
```

- Items are listed in reverse alphabetical order.
- An item is selected with a single key press (no Enter): `1`–`9`, then `a`–`z` for more than 9 items.
- After the directions are shown, the menu appears again. `Esc` exits.

## Data file format

```
+ Walk to the end of the hall.
├──+ Turn left.
|  └──+ Go through the first door on the right.
|     └── Item: Coffee Mug
└──+ Turn right.
   └── Item: Mobile Phone
```

- `+ ` marks a direction, `Item: ` marks an item (items are unique).
- `├──` / `└──` connect a line to its parent; `|` and spaces are indentation.
- Each level is 3 characters wide, so the depth of a line is `connectorIndex / 3 + 1`.

## Error handling

The file is validated line by line. Any problem stops the application with a message
that starts with `FAILURE:` and names the line, for example:

```
FAILURE: Could not load the data file. Line 3: Blank lines are not allowed.
```

Checked cases include a missing file, blank lines, misaligned or malformed connectors,
skipped indentation levels, items with children and duplicate items.

## Project structure

| File | Responsibility |
|------|----------------|
| `Program.cs` | Entry point: picks the data source, runs the menu, reports errors |
| `DataFileSelector.cs` | Lists data files in `TestData` and lets the user pick one |
| `DataFileParser.cs` | Validates the lines and builds the tree (recursive) |
| `DataFileException.cs` | Error in the data file, with the line number |
| `Node.cs` | Tree node: direction or item, with `Parent`, `Children` and `GetDirections()` |
| `ItemMenu.cs` | Item list, single key selection, prints the directions |
| `SelectionKeys.cs` | Maps list positions to keys `1`–`9`, `a`–`z` |
