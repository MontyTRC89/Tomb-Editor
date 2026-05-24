#nullable enable

using System.Collections.Generic;

namespace TombLib.Scripting.UI.Presentation;

public sealed class TextReferenceGroup
{
    public TextReferenceGroup(string filePath, string displayPath, IReadOnlyList<TextReferenceListItem> items)
    {
        FilePath = filePath;
        DisplayPath = displayPath;
        Items = items;
    }

    public string FilePath { get; }

    public string DisplayPath { get; }

    public IReadOnlyList<TextReferenceListItem> Items { get; }

    public int Count => Items.Count;

    public string Header => $"{DisplayPath} ({Count})";
}