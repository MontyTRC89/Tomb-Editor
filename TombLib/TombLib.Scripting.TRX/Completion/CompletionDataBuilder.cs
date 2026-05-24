#nullable enable

using System.Collections.Generic;
using TombLib.Scripting.Completion;

namespace TombLib.Scripting.TRX.Completion;

/// <summary>
/// Helper class to build completion data while avoiding duplicates.
/// </summary>
public sealed class CompletionDataBuilder
{
	private readonly List<TextCompletionItem> _data = new();
	private readonly HashSet<string> _addedTexts = new();

	public bool TryAdd(string text, TextCompletionItemKind kind = TextCompletionItemKind.Generic, string? description = null)
	{
		if (!_addedTexts.Add(text))
			return false;

		_data.Add(new TextCompletionItem(text, text, description, kind: kind));
		return true;
	}

	public IReadOnlyList<TextCompletionItem> Build() => _data;
}
