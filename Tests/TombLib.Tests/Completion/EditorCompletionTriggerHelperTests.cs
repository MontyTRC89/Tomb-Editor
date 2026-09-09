using System.Windows.Input;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public class EditorCompletionTriggerHelperTests
{
	[TestMethod]
	public void IsCtrlSpaceInput_ReturnsTrueOnlyForCtrlSpace()
	{
		Assert.IsTrue(EditorCompletionTriggerHelper.IsCtrlSpaceInput(" ", ModifierKeys.Control));
		Assert.IsTrue(EditorCompletionTriggerHelper.IsCtrlSpaceInput(" ", ModifierKeys.Control | ModifierKeys.Shift));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsCtrlSpaceInput("a", ModifierKeys.Control));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsCtrlSpaceInput(" ", ModifierKeys.None));
	}

	[TestMethod]
	public void IsSingleCharacterLine_HandlesGeneralAndPredicateChecks()
	{
		Assert.IsTrue(EditorCompletionTriggerHelper.IsSingleCharacterLine("a"));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsSingleCharacterLine(string.Empty));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsSingleCharacterLine("ab"));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsSingleCharacterLine(null));

		Assert.IsTrue(EditorCompletionTriggerHelper.IsSingleCharacterLine("a", char.IsLetter));
		Assert.IsFalse(EditorCompletionTriggerHelper.IsSingleCharacterLine("1", char.IsLetter));
	}
}
