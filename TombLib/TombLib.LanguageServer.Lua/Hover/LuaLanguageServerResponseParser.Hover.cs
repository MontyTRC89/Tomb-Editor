using System.Text.Json;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Core.Lua;

namespace TombLib.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses hover content from a LuaLS hover response.
	/// </summary>
	internal static TextHoverInfo? ParseHoverInfo(HoverResponse? response)
	{
		if (response is null || response.Contents.ValueKind == JsonValueKind.Undefined)
			return null;

		MarkupContent hoverContent = MarkupContentReader.ExtractContent(response.Contents);

		return string.IsNullOrWhiteSpace(hoverContent.Text)
			? null
			: new TextHoverInfo(
				hoverContent.IsMarkdown ? hoverContent.Text : hoverContent.Text.Trim(),
				hoverContent.IsMarkdown ? TextHoverContentKind.Markdown : TextHoverContentKind.PlainText);
	}
}
