using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Objects;

namespace TombLib.Test;

[TestClass]
public class GameFlowEditorCompletionWindowTests
{
	[TestMethod]
	public void ShowCompletionWindow_ClearsFieldWhenWindowCloses()
	{
		WpfTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0));
			Window hostWindow = WpfTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WpfTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow completionWindow = WpfTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");
				completionWindow.Close();
				WpfTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Assert.IsNull(WpfTestHelper.FindInstanceField(editor.GetType(), "_completionWindow")?.GetValue(editor));
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void InitializeCompletionWindow_ReplacesVisibleWindowInsteadOfLeavingItOpen()
	{
		WpfTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0));
			Window hostWindow = WpfTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WpfTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow firstWindow = WpfTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WpfTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow secondWindow = WpfTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				Assert.AreNotSame(firstWindow, secondWindow);
				Assert.IsFalse(firstWindow.IsVisible);
				Assert.IsTrue(secondWindow.IsVisible);
			}
			finally
			{
				WpfTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow").Close();
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryOpenCompletionWindow_PopulatesItemsAndOffsets()
	{
		WpfTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0))
			{
				Text = "test"
			};
			Window hostWindow = WpfTestHelper.ShowInHostWindow(editor);

			try
			{
				bool opened = (bool)(WpfTestHelper.InvokeInstanceMethod(
					editor,
					"TryOpenCompletionWindow",
					[typeof(IEnumerable<ICompletionData>), typeof(int?), typeof(int?), typeof(int), typeof(int)],
					new List<ICompletionData> { new CompletionData("Level") },
					1,
					3,
					300,
					300)
					?? throw new InvalidOperationException("Instance method 'TryOpenCompletionWindow' returned null."));

				WpfTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow completionWindow = WpfTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				Assert.IsTrue(opened);
				Assert.AreEqual(1, completionWindow.CompletionList.CompletionData.Count);
				Assert.AreEqual(1, completionWindow.StartOffset);
				Assert.AreEqual(3, completionWindow.EndOffset);
			}
			finally
			{
				WpfTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow").Close();
				hostWindow.Close();
			}
		});
	}
}
