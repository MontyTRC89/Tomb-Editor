using System.Diagnostics.CodeAnalysis;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua;

public static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses a workspace edit from a LuaLS rename response.
	/// </summary>
	public static LuaWorkspaceEdit? ParseWorkspaceEdit(WorkspaceEditResponse? response)
	{
		if (response is null)
			return null;

		var editsByFile = new Dictionary<string, List<LuaTextEdit>>(StringComparer.OrdinalIgnoreCase);

		ParseChangeMap(response.Value.Changes, editsByFile);

		if (!ParseDocumentChanges(response.Value.DocumentChanges, editsByFile))
			return null;

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

	private static void ParseChangeMap(IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? changes,
		Dictionary<string, List<LuaTextEdit>> editsByFile)
	{
		if (changes is null)
			return;

		foreach ((string uri, IReadOnlyList<TextEditPayload>? edits) in changes)
		{
			if (!LanguageServerPathHelper.TryGetFilePath(uri, out string filePath))
				continue;

			List<LuaTextEdit> textEdits = GetOrCreateTextEditBucket(editsByFile, filePath);
			AppendTextEdits(edits, textEdits);
		}
	}

	private static bool ParseDocumentChanges(IReadOnlyList<WorkspaceDocumentChangePayload>? documentChanges,
		Dictionary<string, List<LuaTextEdit>> editsByFile)
	{
		if (documentChanges is null)
			return true;

		for (int i = 0; i < documentChanges.Count; i++)
		{
			WorkspaceDocumentChangePayload documentChange = documentChanges[i];

			if (documentChange.IsResourceOperation)
				return false;

			if (!LanguageServerPathHelper.TryGetFilePath(documentChange.TextDocument?.Uri, out string filePath))
				continue;

			List<LuaTextEdit> textEdits = GetOrCreateTextEditBucket(editsByFile, filePath);
			AppendTextEdits(documentChange.Edits, textEdits);
		}

		return true;
	}

	private static void AppendTextEdits(IReadOnlyList<TextEditPayload>? edits, List<LuaTextEdit> textEdits)
	{
		if (edits is null)
			return;

		for (int i = 0; i < edits.Count; i++)
		{
			if (TryParseTextEdit(edits[i], out LuaTextEdit? textEdit))
				textEdits.Add(textEdit);
		}
	}

	private static bool TryParseTextEdit(TextEditPayload edit, [NotNullWhen(true)] out LuaTextEdit? textEdit)
	{
		textEdit = null;

		if (!ProtocolRangeHelper.TryGetOneBasedRange(edit.Range, out OneBasedDocumentRange? range))
			return false;

		textEdit = new LuaTextEdit(
			new LuaDocumentRange(range.Value.StartLineNumber, range.Value.StartColumnNumber, range.Value.EndLineNumber, range.Value.EndColumnNumber),
			edit.NewText ?? string.Empty);
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
