#nullable enable

using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal static partial class LuaLanguageServerResponseParser
{
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
}
