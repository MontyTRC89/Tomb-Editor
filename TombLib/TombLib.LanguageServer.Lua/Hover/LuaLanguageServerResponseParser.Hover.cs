using System.Text.Json;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses hover content from a LuaLS hover response.
	/// </summary>
	internal static LuaHoverInfo? ParseHoverInfo(HoverResponse? response)
	{
		if (response is null || response.Contents.ValueKind == JsonValueKind.Undefined)
			return null;

		MarkupContent hoverContent = MarkupContentReader.ExtractContent(response.Contents);

		return string.IsNullOrWhiteSpace(hoverContent.Text)
			? null
			: new LuaHoverInfo(hoverContent.IsMarkdown ? hoverContent.Text : hoverContent.Text.Trim(), hoverContent.IsMarkdown);
	}
}
