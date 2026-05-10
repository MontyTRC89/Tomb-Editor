#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses a workspace edit from a LuaLS rename response.
	/// </summary>
	public static LuaWorkspaceEdit? ParseWorkspaceEdit(JsonElement response)
	{
		if (response.ValueKind != JsonValueKind.Object)
			return null;

		var editsByFile = new Dictionary<string, List<LuaTextEdit>>(StringComparer.OrdinalIgnoreCase);

		ParseChangeMap(response, editsByFile);
		ParseDocumentChanges(response, editsByFile);

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

	private static void ParseChangeMap(JsonElement response, Dictionary<string, List<LuaTextEdit>> editsByFile)
	{
		if (!response.TryGetProperty("changes", out JsonElement changesElement)
			|| changesElement.ValueKind != JsonValueKind.Object)
		{
			return;
		}

		foreach (JsonProperty changeProperty in changesElement.EnumerateObject())
		{
			if (!Uri.TryCreate(changeProperty.Name, UriKind.Absolute, out Uri? parsedUri) || !parsedUri.IsFile)
				continue;

			string filePath = LuaLanguageServerPathHelper.NormalizeLocalPath(parsedUri);
			List<LuaTextEdit> textEdits = GetOrCreateTextEditBucket(editsByFile, filePath);
			AppendTextEdits(changeProperty.Value, textEdits);
		}
	}

	private static void ParseDocumentChanges(JsonElement response, Dictionary<string, List<LuaTextEdit>> editsByFile)
	{
		if (!response.TryGetProperty("documentChanges", out JsonElement documentChangesElement)
			|| documentChangesElement.ValueKind != JsonValueKind.Array)
		{
			return;
		}

		foreach (JsonElement documentChangeElement in documentChangesElement.EnumerateArray())
		{
			if (documentChangeElement.ValueKind != JsonValueKind.Object
				|| !documentChangeElement.TryGetProperty("textDocument", out JsonElement textDocumentElement)
				|| !textDocumentElement.TryGetProperty("uri", out JsonElement uriElement))
			{
				continue;
			}

			string? uri = uriElement.GetString();

			if (string.IsNullOrWhiteSpace(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsedUri) || !parsedUri.IsFile)
				continue;

			if (!documentChangeElement.TryGetProperty("edits", out JsonElement editsElement))
				continue;

			string filePath = LuaLanguageServerPathHelper.NormalizeLocalPath(parsedUri);
			List<LuaTextEdit> textEdits = GetOrCreateTextEditBucket(editsByFile, filePath);
			AppendTextEdits(editsElement, textEdits);
		}
	}

	private static void AppendTextEdits(JsonElement editsElement, List<LuaTextEdit> textEdits)
	{
		if (editsElement.ValueKind != JsonValueKind.Array)
			return;

		foreach (JsonElement editElement in editsElement.EnumerateArray())
		{
			if (TryParseTextEdit(editElement, out LuaTextEdit? textEdit))
				textEdits.Add(textEdit);
		}
	}

	private static bool TryParseTextEdit(JsonElement editElement, [NotNullWhen(true)] out LuaTextEdit? textEdit)
	{
		textEdit = null;

		if (editElement.ValueKind != JsonValueKind.Object
			|| !editElement.TryGetProperty("range", out JsonElement rangeElement)
			|| !TryParseRange(rangeElement, out LuaDocumentRange? range)
			|| !editElement.TryGetProperty("newText", out JsonElement newTextElement))
		{
			return false;
		}

		textEdit = new LuaTextEdit(range, newTextElement.GetString() ?? string.Empty);
		return true;
	}

	private static bool TryParseRange(JsonElement rangeElement, [NotNullWhen(true)] out LuaDocumentRange? range)
	{
		range = null;

		if (!rangeElement.TryGetProperty("start", out JsonElement startElement)
			|| !rangeElement.TryGetProperty("end", out JsonElement endElement)
			|| !TryGetLineAndColumn(startElement, out int startLineNumber, out int startColumnNumber)
			|| !TryGetLineAndColumn(endElement, out int endLineNumber, out int endColumnNumber))
		{
			return false;
		}

		range = new LuaDocumentRange(startLineNumber, startColumnNumber, endLineNumber, endColumnNumber);
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