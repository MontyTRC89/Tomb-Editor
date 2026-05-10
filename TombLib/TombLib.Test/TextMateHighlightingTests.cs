using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Lua;

namespace TombLib.Test;

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

		private static (int StartLineIndex, int RemovedLineCount, int InsertedLineCount) ApplyChangeAndGetInfo(string originalText, Action<TextDocument> changeAction)
		{
			var document = new TextDocument(originalText);
			DocumentChangeEventArgs? change = null;

			document.Changed += (_, args) => change = args;
			changeAction(document);

			return TextMateDocumentLineList.GetChangeInfo(document, change!);
		}

		private static string GetForegroundColor(TextMateHighlightingStyle style)
		{
			Assert.IsNotNull(style.Foreground);
			return ((SolidColorBrush)style.Foreground).Color.ToString();
		}
}