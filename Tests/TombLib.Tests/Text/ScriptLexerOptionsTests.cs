using TombLib.Scripting.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Direct tests for <see cref="ScriptLexerOptions"/> defaults and initialization.
/// </summary>
[TestClass]
public class ScriptLexerOptionsTests
{
	[TestMethod]
	public void ClassicScript_Default_ReturnsExpectedOptions()
	{
		ScriptLexerOptions options = ScriptLexerOptions.ClassicScript;

		Assert.AreEqual(";", options.CommentDelimiter);
		Assert.AreEqual('>', options.ContinuationMarker);
		Assert.AreEqual('#', options.DirectivePrefix);
		Assert.AreEqual('[', options.SectionOpenBracket);
		Assert.AreEqual(']', options.SectionCloseBracket);
		Assert.AreEqual('$', options.HexPrefix);
		Assert.AreEqual('"', options.StringQuote);
	}

	[TestMethod]
	public void UninitializedOptions_UseNeutralDefaults()
	{
		ScriptLexerOptions options = default;

		Assert.IsNull(options.CommentDelimiter);
		Assert.AreEqual('\0', options.ContinuationMarker);
		Assert.AreEqual('\0', options.DirectivePrefix);
		Assert.AreEqual('\0', options.SectionOpenBracket);
		Assert.AreEqual('\0', options.SectionCloseBracket);
		Assert.AreEqual('\0', options.HexPrefix);
		Assert.AreEqual('\0', options.StringQuote);
	}

	[TestMethod]
	public void CustomOptions_AreSettableViaInit()
	{
		var options = new ScriptLexerOptions
		{
			CommentDelimiter = "//",
			ContinuationMarker = '\0',
			DirectivePrefix = '\0',
			SectionOpenBracket = '\0',
			SectionCloseBracket = '\0',
			HexPrefix = '\0',
			StringQuote = '"'
		};

		Assert.AreEqual("//", options.CommentDelimiter);
		Assert.AreEqual('\0', options.ContinuationMarker);
		Assert.AreEqual('"', options.StringQuote);
	}
}
