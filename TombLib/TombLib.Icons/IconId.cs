namespace TombLib.Icons;

/// Stable, case-sensitive identifier of an icon, in the form "Category/Name".
public readonly record struct IconId(string Category, string Name)
{
    public string Path => $"{Category}/{Name}";
    public override string ToString() => Path;

    public static IconId Parse(string path)
    {
        var slash = path.IndexOf('/');
        if (slash < 0) throw new System.ArgumentException($"Expected 'Category/Name', got '{path}'", nameof(path));
        return new IconId(path[..slash], path[(slash + 1)..]);
    }
}
