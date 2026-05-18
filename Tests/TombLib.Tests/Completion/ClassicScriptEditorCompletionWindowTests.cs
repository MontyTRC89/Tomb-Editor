using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.ClassicScript;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptEditorCompletionWindowTests
{
	[TestMethod]
	public void HandleAutocompleteOnEmptyLine_OpensCompletionWindowAtLineOffset()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new ClassicScriptEditor(new Version(1, 0))
			{
				Text = string.Empty
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				WPFTestHelper.InvokeInstanceMethod(editor, "HandleAutocompleteOnEmptyLine", Type.EmptyTypes);
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow completionWindow = WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				Assert.IsTrue(completionWindow.CompletionList.CompletionData.Count > 0);
				Assert.AreEqual(0, completionWindow.StartOffset);
			}
			finally
			{
				WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow").Close();
				hostWindow.Close();
			}
		});
	}
}
