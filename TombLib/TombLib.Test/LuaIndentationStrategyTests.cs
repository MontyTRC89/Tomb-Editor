using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Lua.Utils;

namespace TombLib.Test;

[TestClass]
public class LuaIndentationStrategyTests
{
	[TestMethod]
	public void IndentLine_AfterThen_AddsIndentToNewLine()
	{
		var strategy = new LuaAutoIndentationStrategy(new TextEditorOptions
		{
			ConvertTabsToSpaces = true,
			IndentationSize = 4
		});
		var document = new TextDocument("if value then\r\n");

		strategy.IndentLine(document, document.GetLineByNumber(2));

		Assert.AreEqual("if value then\r\n    ", document.Text);
	}

	[TestMethod]
	public void IndentLine_BeforeEnd_DedentsCurrentLineWithoutExtraInsertion()
	{
		var strategy = new LuaAutoIndentationStrategy(new TextEditorOptions
		{
			ConvertTabsToSpaces = true,
			IndentationSize = 4
		});
		var document = new TextDocument("if value then\r\n end");

		strategy.IndentLine(document, document.GetLineByNumber(2));

		Assert.AreEqual("if value then\r\nend", document.Text);
	}

	[TestMethod]
	public void NormalizeCompletionInsertion_DedentsEndLineAndPreservesCaretAndCrLf()
	{
		(string text, int? caretOffset) = LuaIndentationStrategy.NormalizeCompletionInsertion(
			"if condition then\r\n\t\r\nend",
			"if condition then\r\n\t".Length,
			"    ",
			"    ");

		Assert.AreEqual("if condition then\r\n        \r\n    end", text);
		Assert.AreEqual("if condition then\r\n        ".Length, caretOffset);
	}
}