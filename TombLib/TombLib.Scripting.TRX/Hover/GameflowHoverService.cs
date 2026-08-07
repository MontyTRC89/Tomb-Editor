using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Nickelony.LanguageServer.Abstractions.Hover;
using NLog;
using System;
using System.Text.RegularExpressions;
using TombLib.Scripting.Hover;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Hover;

/// <summary>
/// Resolves hover information for GameFlow schema properties.
/// </summary>
public sealed class GameFlowHoverService : ITextHoverProvider
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly Regex WordPattern = new(@"""([^""]+)""|(\w+)", RegexOptions.Compiled);

	private readonly IGameFlowSchemaService _schemaService;

	/// <summary>
	/// Initializes a new instance of the <see cref="GameFlowHoverService"/> class.
	/// </summary>
	/// <param name="schemaService">The schema service used to resolve hover information.</param>
	public GameFlowHoverService(IGameFlowSchemaService schemaService)
		=> _schemaService = schemaService;

	/// <inheritdoc />
	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		try
		{
			var schema = _schemaService.Schema;

			if (schema is null)
				return null;

			// Get the word at the current position
			var wordAtPosition = GetWordAtPosition(request.DocumentText, request.HoveredOffset);

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
		catch (Exception exception)
		{
			Log.Warn(exception, "Failed to resolve GameFlow hover information.");
			return null;
		}
	}

	private static string? GetWordAtPosition(string documentText, int offset)
	{
		if (offset < 0 || offset >= documentText.Length)
			return null;

		int lineStart = documentText.LastIndexOf('\n', offset) + 1;
		int lineEnd = documentText.IndexOf('\n', offset);

		if (lineEnd < 0)
			lineEnd = documentText.Length;
		else if (lineEnd > lineStart && documentText[lineEnd - 1] == '\r')
			lineEnd--;

		string lineText = documentText.Substring(lineStart, lineEnd - lineStart);
		int relativeOffset = offset - lineStart;

		// Use regex to find JSON property names and values
		var matches = WordPattern.Matches(lineText);

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
