using TombLib.Scripting.Completion;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Captures the scope and icon hints inferred from LuaLS completion detail and description text.
/// </summary>
internal readonly struct LuaCompletionTextAnalysis
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaCompletionTextAnalysis"/> struct.
	/// </summary>
	/// <param name="detail">The normalized completion detail text.</param>
	/// <param name="description">The normalized completion description text.</param>
	internal LuaCompletionTextAnalysis(string? detail, string? description)
	{
		HasLocalScope = ContainsToken(detail, "local")
			|| ContainsToken(description, "local");

		HasUpvalueOrParameter = ContainsToken(detail, "upvalue")
			|| ContainsToken(description, "upvalue")
			|| ContainsToken(detail, "parameter")
			|| ContainsToken(description, "parameter");

		KindOverride = ResolveKind(detail);
	}

	/// <summary>
	/// Gets a value indicating whether the completion text suggests a local-scope symbol.
	/// </summary>
	internal bool HasLocalScope { get; }

	/// <summary>
	/// Gets a value indicating whether the completion text suggests an upvalue or parameter symbol.
	/// </summary>
	internal bool HasUpvalueOrParameter { get; }

	/// <summary>
	/// Gets the icon override inferred from the completion detail text, when one can be resolved.
	/// </summary>
	internal TextCompletionItemKind? KindOverride { get; }

	private static bool ContainsToken(string? text, string token)
		=> !string.IsNullOrEmpty(text) && text.Contains(token, StringComparison.OrdinalIgnoreCase);

	private static TextCompletionItemKind? ResolveKind(string? detailText)
	{
		if (ContainsToken(detailText, "parameter"))
			return TextCompletionItemKind.Parameter;

		if (ContainsToken(detailText, "module") || ContainsToken(detailText, "namespace"))
			return TextCompletionItemKind.Namespace;

		if (ContainsToken(detailText, "method") || ContainsToken(detailText, "function"))
			return TextCompletionItemKind.Method;

		if (ContainsToken(detailText, "field"))
			return TextCompletionItemKind.Field;

		if (ContainsToken(detailText, "property") || ContainsToken(detailText, "global")
			|| ContainsToken(detailText, "default library"))
		{
			return TextCompletionItemKind.Property;
		}

		if (ContainsToken(detailText, "constant"))
			return TextCompletionItemKind.Constant;

		if (ContainsToken(detailText, "keyword"))
			return TextCompletionItemKind.Keyword;

		if (ContainsToken(detailText, "class") || ContainsToken(detailText, "interface")
			|| ContainsToken(detailText, "enum") || ContainsToken(detailText, "struct"))
		{
			return TextCompletionItemKind.Class;
		}

		return null;
	}
}
