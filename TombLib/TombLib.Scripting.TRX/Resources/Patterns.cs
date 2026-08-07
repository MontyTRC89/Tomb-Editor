using TombLib.Scripting.TRX.Services;

namespace TombLib.Scripting.TRX.Resources;

/// <summary>
/// Defines the regex patterns used for TRX syntax highlighting.
/// </summary>
public sealed class Patterns
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Patterns"/> class.
	/// </summary>
	/// <param name="schemaService">The schema service used to derive keyword patterns.</param>
	public Patterns(IGameFlowSchemaService schemaService)
	{
		Comments = "//.*$";

		var schemaKeywords = schemaService.GetSchemaKeywords();

		if (schemaKeywords is not null)
		{
			Constants = $"\"\\b({string.Join("|", schemaKeywords.Constants)})\\b\"";
			Collections = $"\"\\b({string.Join("|", schemaKeywords.Collections)})\\b\"";
			Properties = $"\"\\b({string.Join("|", schemaKeywords.Properties)})\\b\"";
		}
		else
		{
			Constants = string.Empty;
			Collections = string.Empty;
			Properties = string.Empty;
		}

		Values = $@"\b({string.Join("|", Keywords.Values)})\b";
		Strings = "\"(.+?)\"";
	}

	/// <summary>
	/// Gets the pattern matching line comments.
	/// </summary>
	public string Comments { get; }

	/// <summary>
	/// Gets the pattern matching constant keywords.
	/// </summary>
	public string Constants { get; }

	/// <summary>
	/// Gets the pattern matching collection keywords.
	/// </summary>
	public string Collections { get; }

	/// <summary>
	/// Gets the pattern matching property keywords.
	/// </summary>
	public string Properties { get; }

	/// <summary>
	/// Gets the pattern matching primitive values.
	/// </summary>
	public string Values { get; }

	/// <summary>
	/// Gets the pattern matching string literals.
	/// </summary>
	public string Strings { get; }

	/// <summary>
	/// Gets the pattern matching a TRX title property.
	/// </summary>
	public static string LevelProperty => "\"title\":\\s*\"";

	/// <summary>
	/// Gets the pattern matching a level-name comment such as <c>// Level 1: Name</c>.
	/// </summary>
	public static string LevelCommentName => @"^\s*\/\/\s*(Level)?\s*\d+\s*(:|\.)\s*(.+)$";
}
