using Nickelony.LanguageServer.Core.Hover;
using System.Text.Json;

namespace Nickelony.LanguageServer.Lua.Tests;

public partial class LuaLanguageServerResponseParserTests
{
	[TestMethod]
	public void ParseHoverInfo_PreservesIndentedMarkdownAndHardBreakWhitespace()
	{
		HoverResponse response = DeserializeHoverResponse(new
		{
			contents = new
			{
				kind = "markdown",
				value = "    local value = 1  \nnext"
			}
		});

		TextHoverInfo? hover = LuaLanguageServerResponseParser.ParseHoverInfo(response);

		Assert.IsNotNull(hover);
		Assert.AreEqual("    local value = 1  \nnext", hover.Content);
		Assert.AreEqual(TextHoverContentKind.Markdown, hover.ContentKind);
	}

	[TestMethod]
	public void ParseHoverInfo_CombinesMarkupArrayWithoutTrimmingIndentedMarkdownFragment()
	{
		HoverResponse response = DeserializeHoverResponse(new
		{
			contents = new object[]
			{
				"Summary",
				new
				{
					kind = "markdown",
					value = "    local value = 1"
				}
			}
		});

		TextHoverInfo? hover = LuaLanguageServerResponseParser.ParseHoverInfo(response);

		Assert.IsNotNull(hover);
		Assert.AreEqual($"Summary{Environment.NewLine}{Environment.NewLine}    local value = 1", hover.Content);
		Assert.AreEqual(TextHoverContentKind.Markdown, hover.ContentKind);
	}

	[TestMethod]
	public void ParseHoverInfo_CodeBlockPayloadUsesFenceLongerThanEmbeddedBackticks()
	{
		HoverResponse response = DeserializeHoverResponse(new
		{
			contents = new
			{
				language = "lua",
				value = "print(\"```\")"
			}
		});

		TextHoverInfo? hover = LuaLanguageServerResponseParser.ParseHoverInfo(response);

		Assert.IsNotNull(hover);
		Assert.AreEqual("````lua\nprint(\"```\")\n````", hover.Content.Replace("\r\n", "\n", StringComparison.Ordinal));
		Assert.AreEqual(TextHoverContentKind.Markdown, hover.ContentKind);
	}

	[TestMethod]
	public void MarkupContentReader_ExtractContent_CombinesMixedArrayAndSkipsMalformedEntries()
	{
		JsonElement element = JsonSerializer.SerializeToElement(new object[]
		{
			"Summary",
			new
			{
				kind = "markdown",
				value = "**bold**"
			},
			new
			{
				value = 5
			},
			new
			{
				language = "lua",
				value = "print(1)"
			},
			new
			{
				value = "tail"
			}
		});

		MarkupContent content = MarkupContentReader.ExtractContent(element);

		Assert.IsTrue(content.IsMarkdown);

		Assert.AreEqual(
			"Summary\n\n**bold**\n\n```lua\nprint(1)\n```\n\ntail",
			content.Text.Replace("\r\n", "\n", StringComparison.Ordinal));
	}

	[TestMethod]
	public void MarkupContentReader_ExtractContent_FallsBackToPlainValueWhenKindHasWrongType()
	{
		JsonElement element = JsonSerializer.SerializeToElement(new
		{
			kind = 5,
			value = "plain text"
		});

		MarkupContent content = MarkupContentReader.ExtractContent(element);

		Assert.AreEqual("plain text", content.Text);
		Assert.IsFalse(content.IsMarkdown);
	}

	[TestMethod]
	public void MarkupContentReader_ExtractContent_ReturnsDefaultForPartiallyMissingCodeBlockPayload()
	{
		JsonElement element = JsonSerializer.SerializeToElement(new
		{
			language = "lua"
		});

		MarkupContent content = MarkupContentReader.ExtractContent(element);

		Assert.IsTrue(string.IsNullOrEmpty(content.Text));
		Assert.IsFalse(content.IsMarkdown);
	}

	[TestMethod]
	public void NormalizeMarkupText_PreservesInlineBackticksForPlainText()
	{
		string? normalized = LuaMarkupTextHelper.NormalizeMarkupText("Call `value` before `other`.");
		Assert.AreEqual("Call `value` before `other`.", normalized);
	}

	[TestMethod]
	public void NormalizeMarkupText_StripsFenceLinesButPreservesCodeContent()
	{
		string? normalized = LuaMarkupTextHelper.NormalizeMarkupText(
			"Summary\n```lua\nlocal value = 1\n```\nTail");

		Assert.AreEqual(
			$"Summary{Environment.NewLine}local value = 1{Environment.NewLine}Tail",
			normalized);
	}
}
