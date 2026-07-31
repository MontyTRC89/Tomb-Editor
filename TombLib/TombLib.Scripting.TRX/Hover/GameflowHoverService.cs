#nullable enable

using ICSharpCode.AvalonEdit.Document;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Text.RegularExpressions;
using Nickelony.LanguageServer.Core.Hover;
using TombLib.Scripting.Hover;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Hover;

public sealed class GameflowHoverService : ITextHoverProvider
{
	private static readonly Regex _wordPattern = new(@"""([^""]+)""|(\w+)", RegexOptions.Compiled);

	private readonly IGameflowSchemaService _schemaService;

	public GameflowHoverService(IGameflowSchemaService schemaService)
		=> _schemaService = schemaService;

	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		try
		{
			var schema = _schemaService.Schema;

			if (schema is null)
				return null;

			var document = new TextDocument(request.DocumentText);

			// Get the word at the current position
			var wordAtPosition = GetWordAtPosition(document, request.HoveredOffset);

			if (string.IsNullOrWhiteSpace(wordAtPosition))
				return null;

			// Clean the word (remove quotes if present)
			var cleanWord = wordAtPosition.Trim('"');

			// Try to find schema information for this word
			string? content = FindHoverInfo(schema, cleanWord);
			return string.IsNullOrWhiteSpace(content)
				? null
				: new TextHoverInfo(content, TextHoverContentKind.Markdown, cleanWord);
		}
		catch
		{
			return null;
		}
	}

	private static string? GetWordAtPosition(IDocument document, int offset)
	{
		if (offset < 0 || offset >= document.TextLength)
			return null;

		var line = document.GetLineByOffset(offset);
		var lineText = document.GetText(line.Offset, line.Length);
		var relativeOffset = offset - line.Offset;

		// Use regex to find JSON property names and values
		var matches = _wordPattern.Matches(lineText);

		foreach (Match match in matches)
		{
			if (relativeOffset >= match.Index && relativeOffset <= match.Index + match.Length)
			{
				// Return the captured group (without quotes for quoted strings)
				return match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
			}
		}

		return null;
	}

	private static string? FindHoverInfo(JSchema schema, string word)
	{
		// First check main schema properties
		if (schema.Properties?.ContainsKey(word) == true)
		{
			var property = schema.Properties[word];
			return FormatPropertyInfo(word, property);
		}

		// Check definitions section
		if (schema.ExtensionData?.ContainsKey("definitions") != true || schema.ExtensionData["definitions"] is not JObject definitions)
			return null;

		foreach (var definition in definitions)
		{
			var defSchema = definition.Value?.ToObject<JSchema>();

			if (defSchema?.Properties?.ContainsKey(word) != true)
				continue;

			var property = defSchema.Properties[word];
			return FormatPropertyInfo(word, property);
		}

		return null;
	}

	private static string FormatPropertyInfo(string propertyName, JSchema propertySchema)
	{
		var info = $"`\"{propertyName}\"`";

		if (propertySchema.Type.HasValue)
			info += $"\nType: `{propertySchema.Type.Value}`";

		if (!string.IsNullOrEmpty(propertySchema.Description))
			info += $"\n\n{propertySchema.Description}";

		return info;
	}
}
