#nullable enable

using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.Objects;

internal sealed class LuaReferenceGroup
{
	public LuaReferenceGroup(string filePath, string displayPath, IReadOnlyList<LuaReferenceListItem> items)
	{
		FilePath = filePath;
		DisplayPath = displayPath;
		Items = items;
	}

	public string FilePath { get; }

	public string DisplayPath { get; }

	public IReadOnlyList<LuaReferenceListItem> Items { get; }

	public int Count => Items.Count;

	public string Header => $"{DisplayPath} ({Count})";
}