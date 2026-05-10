using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.LanguageServer;

public static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses hover content from a LuaLS hover response.
	/// </summary>
	public static LuaHoverInfo? ParseHoverInfo(LuaHoverResponse? response)
	{
		if (response is null || response.Contents.ValueKind == JsonValueKind.Undefined)
			return null;

		MarkupContent hoverContent = ExtractMarkupContent(response.Contents);

		return string.IsNullOrWhiteSpace(hoverContent.Text)
			? null
			: new LuaHoverInfo(hoverContent.Text.Trim(), hoverContent.IsMarkdown);
	}
}
