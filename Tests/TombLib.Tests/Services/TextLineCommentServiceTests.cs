using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.UI.Editing;

namespace TombLib.Tests;

[TestClass]
public class TextLineCommentServiceTests
{
	[TestMethod]
	public void TryCreateEdit_ToggleOnUncommentedSelection_CommentsEachLine()
	{
		var document = new TextDocument("first" + Environment.NewLine + "  second");
		var service = new TextLineCommentService();

		bool success = service.TryCreateEdit(
			document,
			selectionStart: 0,
			selectionLength: document.TextLength,
			commentPrefix: "//",
			action: TextLineCommentAction.Toggle,
			out TextLineCommentEdit edit);

		Assert.IsTrue(success);
		document.Replace(edit.ReplaceOffset, edit.ReplaceLength, edit.ReplacementText);
		Assert.AreEqual("//first" + Environment.NewLine + "  //second" + Environment.NewLine, document.Text);
	}

	[TestMethod]
	public void TryCreateEdit_ToggleOnCommentedSelection_UncommentsEachLine()
	{
		var document = new TextDocument("//first" + Environment.NewLine + "  //second");
		var service = new TextLineCommentService();

		bool success = service.TryCreateEdit(
			document,
			selectionStart: 0,
			selectionLength: document.TextLength,
			commentPrefix: "//",
			action: TextLineCommentAction.Toggle,
			out TextLineCommentEdit edit);

		Assert.IsTrue(success);
		document.Replace(edit.ReplaceOffset, edit.ReplaceLength, edit.ReplacementText);
		Assert.AreEqual("first" + Environment.NewLine + "  second" + Environment.NewLine, document.Text);
	}
}