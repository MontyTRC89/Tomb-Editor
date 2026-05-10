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
	/// Parses reference locations from a LuaLS references response.
	/// </summary>
	public static IReadOnlyList<LuaReferenceLocation> ParseReferenceLocations(JsonElement response)
	{
		if (response.ValueKind != JsonValueKind.Array)
			return [];

		var locations = new List<LuaReferenceLocation>();

		foreach (JsonElement referenceElement in response.EnumerateArray())
		{
			if (referenceElement.ValueKind != JsonValueKind.Object)
				continue;

			if (!TryGetReferenceUri(referenceElement, out Uri? parsedUri)
				|| !TryGetReferenceRange(referenceElement, out LuaDocumentRange? range))
			{
				continue;
			}

			locations.Add(new LuaReferenceLocation(LuaLanguageServerPathHelper.NormalizeLocalPath(parsedUri), range));
		}

		return locations;
	}

	private static bool TryGetReferenceUri(JsonElement referenceElement, [NotNullWhen(true)] out Uri? parsedUri)
	{
		parsedUri = null;

		if (!referenceElement.TryGetProperty("uri", out JsonElement uriElement))
			return false;

		string? uri = uriElement.GetString();
		return !string.IsNullOrWhiteSpace(uri) && Uri.TryCreate(uri, UriKind.Absolute, out parsedUri) && parsedUri.IsFile;
	}

	private static bool TryGetReferenceRange(JsonElement referenceElement, [NotNullWhen(true)] out LuaDocumentRange? range)
	{
		range = null;

		if (!referenceElement.TryGetProperty("range", out JsonElement rangeElement)
			|| !rangeElement.TryGetProperty("start", out JsonElement startElement)
			|| !rangeElement.TryGetProperty("end", out JsonElement endElement))
		{
			return false;
		}

		if (!TryGetLineAndColumn(startElement, out int startLineNumber, out int startColumnNumber)
			|| !TryGetLineAndColumn(endElement, out int endLineNumber, out int endColumnNumber))
		{
			return false;
		}

		range = new LuaDocumentRange(startLineNumber, startColumnNumber, endLineNumber, endColumnNumber);
		return true;
	}

	private static bool TryGetLineAndColumn(JsonElement positionElement, out int lineNumber, out int columnNumber)
	{
		lineNumber = 1;
		columnNumber = 1;

		if (!positionElement.TryGetProperty("line", out JsonElement lineElement)
			|| !lineElement.TryGetInt32(out int parsedLine)
			|| !positionElement.TryGetProperty("character", out JsonElement characterElement)
			|| !characterElement.TryGetInt32(out int parsedCharacter))
		{
			return false;
		}

		lineNumber = parsedLine + 1;
		columnNumber = parsedCharacter + 1;
		return true;
	}
}