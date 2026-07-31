using System.Text.Json;
using Nickelony.LanguageServer.Core.Hover;

namespace Nickelony.LanguageServer.Lua;

internal static partial class LuaLanguageServerResponseParser
{
	/// <summary>
	/// Parses hover content from a LuaLS hover response.
	/// </summary>
	/// <param name="response">The hover response payload, or <see langword="null"/> when unavailable.</param>
	/// <returns>The parsed hover info, or <see langword="null"/> when no usable content is present.</returns>
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
