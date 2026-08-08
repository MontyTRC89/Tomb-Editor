using Nickelony.LanguageServer.Abstractions.Hover;
using NLog;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.Hover;
using TombLib.Scripting.TRX.Models;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Hover;

/// <summary>
/// Resolves hover information for GameFlow schema properties.
/// </summary>
public sealed class TRXGameFlowHoverService : ITextHoverProvider
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly Regex WordPattern = new(@"""([^""]+)""|(\w+)", RegexOptions.Compiled);

	private readonly ITRXGameFlowSchemaService _schemaService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TRXGameFlowHoverService"/> class.
	/// </summary>
	/// <param name="schemaService">The schema service used to resolve hover information.</param>
	public TRXGameFlowHoverService(ITRXGameFlowSchemaService schemaService)
		=> _schemaService = schemaService;

	/// <inheritdoc />
	public TextHoverInfo? GetHoverInfo(TextHoverRequest request)
	{
		try
		{
			var model = _schemaService.Model;

			if (model is null)
				return null;

			// Get the word at the current position.
			var wordAtPosition = GetWordAtPosition(request.DocumentText, request.HoveredOffset);

			if (string.IsNullOrWhiteSpace(wordAtPosition))
				return null;

			// Clean the word (remove quotes if present).
			var cleanWord = wordAtPosition.Trim('"');

			// Try to find schema information for this word.
			string? content = FindHoverInfo(model, cleanWord);
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

		// Use regex to find JSON property names and values.
		var matches = WordPattern.Matches(lineText);

		foreach (Match match in matches)
		{
			if (relativeOffset >= match.Index && relativeOffset <= match.Index + match.Length)
			{
				// Return the captured group (without quotes for quoted strings).
				return match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
			}
		}

		return null;
	}

	private static string? FindHoverInfo(TRXGameFlowSchemaModel model, string word)
	{
		var property = model.Properties.FirstOrDefault(candidate => candidate.Name == word);

		if (property is null)
			return null;

		return FormatPropertyInfo(property);
	}

	private static string FormatPropertyInfo(TRXGameFlowProperty property)
	{
		var info = $"`\"{property.Name}\"`";

		if (property.Types.Count > 0)
			info += $"\nType: `{string.Join(", ", property.Types)}`";

		if (!string.IsNullOrEmpty(property.Description))
			info += $"\n\n{property.Description}";

		return info;
	}
}
