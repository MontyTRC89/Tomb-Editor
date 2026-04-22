#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static partial class LuaLanguageServerResponseParser
{
	private static readonly string[] DefinitionRangeProperties =
	[
		"targetSelectionRange",
		"targetRange",
		"range"
	];

	/// <summary>
	/// Parses hover content from a LuaLS hover response.
	/// </summary>
	public static LuaHoverInfo? ParseHoverInfo(JsonElement response)
	{
		if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("contents", out JsonElement contentsElement))
			return null;

		MarkupContent hoverContent = ExtractMarkupContent(contentsElement);

		return string.IsNullOrWhiteSpace(hoverContent.Text)
			? null
			: new LuaHoverInfo(hoverContent.Text.Trim(), hoverContent.IsMarkdown);
	}

	/// <summary>
	/// Parses a definition location from a LuaLS definition response.
	/// </summary>
	public static LuaDefinitionLocation? ParseDefinitionLocation(JsonElement response)
	{
		JsonElement definitionElement = response;

		if (response.ValueKind == JsonValueKind.Array)
		{
			definitionElement = response.EnumerateArray().FirstOrDefault();

			if (definitionElement.ValueKind == JsonValueKind.Undefined)
				return null;
		}

		if (definitionElement.ValueKind != JsonValueKind.Object)
			return null;

		if (!TryGetDefinitionUri(definitionElement, out Uri? parsedUri))
			return null;

		if (!TryGetDefinitionStartElement(definitionElement, out JsonElement startElement))
			return null;

		int lineNumber = startElement.TryGetProperty("line", out JsonElement lineElement)
			&& lineElement.TryGetInt32(out int parsedLine) ? parsedLine + 1 : 1;
		int columnNumber = startElement.TryGetProperty("character", out JsonElement characterElement)
			&& characterElement.TryGetInt32(out int parsedCharacter) ? parsedCharacter + 1 : 1;

		return new LuaDefinitionLocation(LuaLanguageServerPathHelper.NormalizeLocalPath(parsedUri), lineNumber, columnNumber);
	}

	private static bool TryGetDefinitionUri(JsonElement definitionElement, [NotNullWhen(true)] out Uri? parsedUri)
	{
		parsedUri = null;

		string? uri = definitionElement.TryGetProperty("targetUri", out JsonElement targetUriElement)
			? targetUriElement.GetString()
			: definitionElement.TryGetProperty("uri", out JsonElement uriElement)
				? uriElement.GetString()
				: null;

		return !string.IsNullOrWhiteSpace(uri) && Uri.TryCreate(uri, UriKind.Absolute, out parsedUri) && parsedUri.IsFile;
	}

	private static bool TryGetDefinitionStartElement(JsonElement definitionElement, out JsonElement startElement)
	{
		foreach (string rangeProperty in DefinitionRangeProperties)
		{
			if (definitionElement.TryGetProperty(rangeProperty, out JsonElement rangeElement)
				&& rangeElement.TryGetProperty("start", out startElement)
				&& startElement.ValueKind == JsonValueKind.Object)
			{
				return true;
			}
		}

		startElement = default;
		return false;
	}
}
