using TombLib.Scripting.Lua.Objects;

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

		IconKindOverride = ResolveIconKind(detail);
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
	internal LuaCompletionIconKind? IconKindOverride { get; }

	private static bool ContainsToken(string? text, string token)
		=> !string.IsNullOrEmpty(text) && text.Contains(token, StringComparison.OrdinalIgnoreCase);

	private static LuaCompletionIconKind? ResolveIconKind(string? detailText)
	{
		if (ContainsToken(detailText, "parameter"))
			return LuaCompletionIconKind.Parameter;

		if (ContainsToken(detailText, "module") || ContainsToken(detailText, "namespace"))
			return LuaCompletionIconKind.Namespace;

		if (ContainsToken(detailText, "method") || ContainsToken(detailText, "function"))
			return LuaCompletionIconKind.Method;

		if (ContainsToken(detailText, "field"))
			return LuaCompletionIconKind.Field;

		if (ContainsToken(detailText, "property") || ContainsToken(detailText, "global")
			|| ContainsToken(detailText, "default library"))
		{
			return LuaCompletionIconKind.Property;
		}

		if (ContainsToken(detailText, "constant"))
			return LuaCompletionIconKind.Constant;

		if (ContainsToken(detailText, "keyword"))
			return LuaCompletionIconKind.Keyword;

		if (ContainsToken(detailText, "class") || ContainsToken(detailText, "interface")
			|| ContainsToken(detailText, "enum") || ContainsToken(detailText, "struct"))
		{
			return LuaCompletionIconKind.Class;
		}

		return null;
	}
}
