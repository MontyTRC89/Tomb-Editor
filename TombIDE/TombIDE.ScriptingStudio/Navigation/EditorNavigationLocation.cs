#nullable enable

using System;

namespace TombIDE.ScriptingStudio.Navigation;

internal readonly record struct EditorNavigationLocation(
	string FilePath,
	int CaretOffset,
	int SelectionStart,
	int SelectionLength,
	int? PreferredLine)
{
	public bool IsEquivalentTo(EditorNavigationLocation other)
		=> string.Equals(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase)
			&& CaretOffset == other.CaretOffset
			&& SelectionStart == other.SelectionStart
			&& SelectionLength == other.SelectionLength;
}