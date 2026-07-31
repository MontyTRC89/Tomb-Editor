using System.Diagnostics.CodeAnalysis;
using TombLib.Scripting.Editing;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses a workspace edit from a LuaLS rename response.
	/// </summary>
	/// <param name="response">The workspace edit response payload, or <see langword="null"/> when unavailable.</param>
	/// <param name="logger">The logger instance, or <see langword="null"/> for no logging.</param>
	/// <returns>The parsed workspace edit, or <see langword="null"/> when no edits are present.</returns>
	internal static TextWorkspaceEdit? ParseWorkspaceEdit(WorkspaceEditResponse? response, ILogger? logger = null)
	{
		if (response is null)
			return null;

		var editsByFile = new Dictionary<string, List<TextEdit>>(StringComparer.OrdinalIgnoreCase);

		ParseChangeMap(response.Value.Changes, editsByFile);

		if (!ParseDocumentChanges(response.Value.DocumentChanges, editsByFile, logger))
			return null;

		if (editsByFile.Count == 0)
			return null;

		var documentEdits = new List<TextDocumentEdit>(editsByFile.Count);

		foreach ((string filePath, List<TextEdit> textEdits) in editsByFile)
		{
			if (textEdits.Count == 0)
				continue;

			documentEdits.Add(new TextDocumentEdit(filePath, textEdits));
		}

		return documentEdits.Count == 0
			? null
			: new TextWorkspaceEdit(documentEdits);
	}

	private static void ParseChangeMap(IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? changes,
		Dictionary<string, List<TextEdit>> editsByFile)
	{
		if (changes is null)
			return;

		foreach ((string uri, IReadOnlyList<TextEditPayload>? edits) in changes)
		{
			if (!LanguageServerPathHelper.TryGetFilePath(uri, out string filePath))
				continue;

			List<TextEdit> textEdits = GetOrCreateTextEditBucket(editsByFile, filePath);
			AppendTextEdits(edits, textEdits);
		}
	}

	private static bool ParseDocumentChanges(IReadOnlyList<WorkspaceDocumentChangePayload>? documentChanges,
		Dictionary<string, List<TextEdit>> editsByFile, ILogger? logger = null)
	{
		if (documentChanges is null)
			return true;

		for (int i = 0; i < documentChanges.Count; i++)
		{
			WorkspaceDocumentChangePayload documentChange = documentChanges[i];

			if (documentChange.IsResourceOperation)
			{
				logger?.LogWarning(
					"Ignoring Lua rename workspace edit because it contains unsupported resource operation '{Kind}' (uri: '{Uri}', oldUri: '{OldUri}', newUri: '{NewUri}').",
					documentChange.Kind,
					documentChange.Uri ?? string.Empty,
					documentChange.OldUri ?? string.Empty,
					documentChange.NewUri ?? string.Empty);

				return false; // Resource operations are currently unsupported by the editor-side rename flow, so fail closed.
			}

			if (!LanguageServerPathHelper.TryGetFilePath(documentChange.TextDocument?.Uri, out string filePath))
				continue;

			List<TextEdit> textEdits = GetOrCreateTextEditBucket(editsByFile, filePath);
			AppendTextEdits(documentChange.Edits, textEdits);
		}

		return true;
	}

	private static void AppendTextEdits(IReadOnlyList<TextEditPayload>? edits, List<TextEdit> textEdits)
	{
		if (edits is null)
			return;

		for (int i = 0; i < edits.Count; i++)
		{
			if (TryParseTextEdit(edits[i], out TextEdit? textEdit))
				textEdits.Add(textEdit);
		}
	}

	private static bool TryParseTextEdit(TextEditPayload edit, [NotNullWhen(true)] out TextEdit? textEdit)
	{
		textEdit = null;

		if (!ProtocolRangeHelper.TryGetOneBasedRange(edit.Range, out OneBasedDocumentRange? range))
			return false;

		textEdit = new TextEdit(
			new TextDocumentRange(range.Value.StartLineNumber, range.Value.StartColumnNumber, range.Value.EndLineNumber, range.Value.EndColumnNumber),
			edit.NewText ?? string.Empty);

		return true;
	}

	private static List<TextEdit> GetOrCreateTextEditBucket(Dictionary<string, List<TextEdit>> editsByFile, string filePath)
	{
		if (!editsByFile.TryGetValue(filePath, out List<TextEdit>? textEdits))
		{
			textEdits = [];
			editsByFile[filePath] = textEdits;
		}

		return textEdits;
	}
}
