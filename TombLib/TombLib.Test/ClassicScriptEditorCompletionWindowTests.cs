using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.ClassicScript;

namespace TombLib.Test;

[TestClass]
public class ClassicScriptEditorCompletionWindowTests
{
	[TestMethod]
	public void HandleAutocompleteOnEmptyLine_OpensCompletionWindowAtLineOffset()
	{
		WpfTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0))
			{
				Text = string.Empty
			};
			Window hostWindow = WpfTestHelper.ShowInHostWindow(editor);

			try
			{
				WpfTestHelper.InvokeInstanceMethod(editor, "HandleAutocompleteOnEmptyLine", Type.EmptyTypes);
				WpfTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow completionWindow = WpfTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				Assert.IsTrue(completionWindow.CompletionList.CompletionData.Count > 0);
				Assert.AreEqual(0, completionWindow.StartOffset);
			}
			finally
			{
				WpfTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow").Close();
				hostWindow.Close();
			}
		});
	}
}
