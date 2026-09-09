using ICSharpCode.AvalonEdit.Document;
using System.Windows.Media;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Tests;

[TestClass]
public class TextMateHighlightingTests
{
	[TestMethod]
	public void GetChangeInfo_CountsCrLfAsTwoAffectedLines()
	{
		(int startLineIndex, int removedLineCount, int insertedLineCount) = ApplyChangeAndGetInfo("alpha", document => document.Insert(5, "\r\nbeta"));

		Assert.AreEqual(0, startLineIndex);
		Assert.AreEqual(1, removedLineCount);
		Assert.AreEqual(2, insertedLineCount);
	}

	[TestMethod]
	public void GetChangeInfo_CountsLfAsTwoAffectedLines()
	{
		(int startLineIndex, int removedLineCount, int insertedLineCount) = ApplyChangeAndGetInfo("alpha", document => document.Insert(5, "\nbeta"));

		Assert.AreEqual(0, startLineIndex);
		Assert.AreEqual(1, removedLineCount);
		Assert.AreEqual(2, insertedLineCount);
	}

	[TestMethod]
	public void GetChangeInfo_CountsLoneCrAsTwoAffectedLines()
	{
		(int startLineIndex, int removedLineCount, int insertedLineCount) = ApplyChangeAndGetInfo("alpha", document => document.Insert(5, "\rbeta"));

		Assert.AreEqual(0, startLineIndex);
		Assert.AreEqual(1, removedLineCount);
		Assert.AreEqual(2, insertedLineCount);
	}

	[TestMethod]
	public void Resolve_UsesCommaSeparatedBundledFunctionSelectors()
	{
		var resolver = new TextMateThemeStyleResolver(new LuaEditorConfiguration
		{
			SelectedThemeName = "Tomorrow Light"
		}.Theme.TextMateTheme);

		TextMateHighlightingStyle style = resolver.Resolve(["source.lua", "support.function.library.lua"]);

		Assert.AreEqual("#FF4271AE", GetForegroundColor(style));
	}

	[TestMethod]
	public void Resolve_PrefersMoreSpecificBundledSelectorsOverBroaderParents()
	{
		var resolver = new TextMateThemeStyleResolver(new LuaEditorConfiguration
		{
			SelectedThemeName = "SharpLua"
		}.Theme.TextMateTheme);

		TextMateHighlightingStyle style = resolver.Resolve(["source.lua", "support.type.property-name.lua"]);

		Assert.AreEqual("#FFD7B8FF", GetForegroundColor(style));
	}

	[TestMethod]
	public void DocumentLineList_TracksInsertedAndReplacedLines()
	{
		var document = new TextDocument("alpha\r\nbeta");
		var lineList = new TextMateDocumentLineList(document);

		try
		{
			CollectionAssert.AreEqual(new[] { "alpha\r\n", "beta" }, GetSnapshotLines(lineList));

			document.Insert(document.TextLength, "\r\ngamma");

			CollectionAssert.AreEqual(new[] { "alpha\r\n", "beta\r\n", "gamma" }, GetSnapshotLines(lineList));
			Assert.AreEqual(3, lineList.GetNumberOfLines());

			int replacementOffset = document.Text.IndexOf("beta\r\ngamma", StringComparison.Ordinal);
			document.Replace(replacementOffset, "beta\r\ngamma".Length, "delta");

			CollectionAssert.AreEqual(new[] { "alpha\r\n", "delta" }, GetSnapshotLines(lineList));
			Assert.AreEqual(2, lineList.GetNumberOfLines());
		}
		finally
		{
			lineList.Dispose();
		}
	}

	private static (int StartLineIndex, int RemovedLineCount, int InsertedLineCount) ApplyChangeAndGetInfo(string originalText, Action<TextDocument> changeAction)
	{
		var document = new TextDocument(originalText);
		DocumentChangeEventArgs? change = null;

		document.Changed += (_, args) => change = args;
		changeAction(document);

		return TextMateDocumentLineList.GetChangeInfo(document, change!);
	}

	private static string[] GetSnapshotLines(TextMateDocumentLineList lineList)
		=> [.. WPFTestHelper.GetPrivateField<List<string>>(lineList, "_lineTexts")];

	private static string GetForegroundColor(TextMateHighlightingStyle style)
	{
		Assert.IsNotNull(style.Foreground);
		return ((SolidColorBrush)style.Foreground).Color.ToString();
	}
}
