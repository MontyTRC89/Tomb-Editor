using System.Diagnostics.CodeAnalysis;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.LanguageServer;

public static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses a workspace edit from a LuaLS rename response.
	/// </summary>
	public static LuaWorkspaceEdit? ParseWorkspaceEdit(LuaWorkspaceEditResponse? response)
	{
		if (response is null)
			return null;

		var editsByFile = new Dictionary<string, List<LuaTextEdit>>(StringComparer.OrdinalIgnoreCase);

		ParseChangeMap(response.Value.Changes, editsByFile);
		ParseDocumentChanges(response.Value.DocumentChanges, editsByFile);

		if (editsByFile.Count == 0)
			return null;

		var documentEdits = new List<LuaDocumentEdit>(editsByFile.Count);

		foreach ((string filePath, List<LuaTextEdit> textEdits) in editsByFile)
		{
			if (textEdits.Count == 0)
				continue;

			documentEdits.Add(new LuaDocumentEdit(filePath, textEdits));
		}

		return documentEdits.Count == 0
			? null
			: new LuaWorkspaceEdit(documentEdits);
	}

	private static void ParseChangeMap(IReadOnlyDictionary<string, LuaTextEditPayload[]?>? changes,
		Dictionary<string, List<LuaTextEdit>> editsByFile)
	{
		if (changes is null)
			return;

		foreach ((string uri, LuaTextEditPayload[]? edits) in changes)
		{
			if (!LuaLanguageServerPathHelper.TryGetFilePath(uri, out string filePath))
				continue;

			List<LuaTextEdit> textEdits = GetOrCreateTextEditBucket(editsByFile, filePath);
			AppendTextEdits(edits, textEdits);
		}
	}

	private static void ParseDocumentChanges(IReadOnlyList<LuaWorkspaceDocumentChangePayload>? documentChanges,
		Dictionary<string, List<LuaTextEdit>> editsByFile)
	{
		if (documentChanges is null)
			return;

		for (int i = 0; i < documentChanges.Count; i++)
		{
			LuaWorkspaceDocumentChangePayload documentChange = documentChanges[i];

			if (!LuaLanguageServerPathHelper.TryGetFilePath(documentChange.TextDocument?.Uri, out string filePath))
				continue;

			List<LuaTextEdit> textEdits = GetOrCreateTextEditBucket(editsByFile, filePath);
			AppendTextEdits(documentChange.Edits, textEdits);
		}
	}

	private static void AppendTextEdits(IReadOnlyList<LuaTextEditPayload>? edits, List<LuaTextEdit> textEdits)
	{
		if (edits is null)
			return;

		for (int i = 0; i < edits.Count; i++)
		{
			if (TryParseTextEdit(edits[i], out LuaTextEdit? textEdit))
				textEdits.Add(textEdit);
		}
	}

	private static bool TryParseTextEdit(LuaTextEditPayload edit, [NotNullWhen(true)] out LuaTextEdit? textEdit)
	{
		textEdit = null;

		if (!TryParseDocumentRange(edit.Range, out LuaDocumentRange? range))
			return false;

		textEdit = new LuaTextEdit(range, edit.NewText ?? string.Empty);
		return true;
	}

	private static List<LuaTextEdit> GetOrCreateTextEditBucket(Dictionary<string, List<LuaTextEdit>> editsByFile, string filePath)
	{
		if (!editsByFile.TryGetValue(filePath, out List<LuaTextEdit>? textEdits))
		{
			textEdits = [];
			editsByFile[filePath] = textEdits;
		}

		return textEdits;
	}
}
