using TombLib.Scripting.Lua.Parsing;

namespace TombLib.Tests;

[TestClass]
public class LuaLineParserTests
{
	[TestMethod]
	public void IsInsideCommentOrString_ReturnsTrueInsideLongComment()
	{
		bool result = LuaLineParser.IsInsideCommentOrString("--[[ comment");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsInsideCommentOrString_ReturnsTrueInsideLongString()
	{
		bool result = LuaLineParser.IsInsideCommentOrString("value = [[comment");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void StripLineComment_RemovesInlineLongCommentAndKeepsCodeAfterIt()
	{
		string result = LuaLineParser.StripLineComment("value = 1 --[[ remove this ]] + 2");
		Assert.AreEqual("value = 1  + 2", result);
	}

	[TestMethod]
	public void EnumerateStructuralCharacters_SkipsLongStringContents()
	{
		string result = new(LuaLineParser.EnumerateStructuralCharacters("{ [[ignored } text]] }").ToArray());
		Assert.AreEqual("{  }", result);
	}

	[TestMethod]
	public void ExtractCodeText_RemovesQuotedAndCommentText()
	{
		string result = LuaLineParser.ExtractCodeText("if value == \"then\" then -- comment");
		Assert.AreEqual("if value ==  then ", result);
	}
}
