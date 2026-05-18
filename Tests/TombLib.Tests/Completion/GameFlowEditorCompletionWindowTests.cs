using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Objects;

namespace TombLib.Tests;

[TestClass]
public class GameFlowEditorCompletionWindowTests
{
	[TestMethod]
	public void ShowCompletionWindow_ClearsFieldWhenWindowCloses()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0));
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow completionWindow = WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");
				completionWindow.Close();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Assert.IsNull(WPFTestHelper.FindInstanceField(editor.GetType(), "_completionWindow")?.GetValue(editor));
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
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0));
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow firstWindow = WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow secondWindow = WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				Assert.AreNotSame(firstWindow, secondWindow);
				Assert.IsFalse(firstWindow.IsVisible);
				Assert.IsTrue(secondWindow.IsVisible);
			}
			finally
			{
				WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow").Close();
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryOpenCompletionWindow_PopulatesItemsAndOffsets()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0))
			{
				Text = "test"
			};
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				bool opened = (bool)(WPFTestHelper.InvokeInstanceMethod(
					editor,
					"TryOpenCompletionWindow",
					[typeof(IEnumerable<ICompletionData>), typeof(int?), typeof(int?), typeof(int), typeof(int)],
					new List<ICompletionData> { new CompletionData("Level") },
					1,
					3,
					300,
					300)
					?? throw new InvalidOperationException("Instance method 'TryOpenCompletionWindow' returned null."));

				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow completionWindow = WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				Assert.IsTrue(opened);
				Assert.AreEqual(1, completionWindow.CompletionList.CompletionData.Count);
				Assert.AreEqual(1, completionWindow.StartOffset);
				Assert.AreEqual(3, completionWindow.EndOffset);
			}
			finally
			{
				WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow").Close();
				hostWindow.Close();
			}
		});
	}
}
