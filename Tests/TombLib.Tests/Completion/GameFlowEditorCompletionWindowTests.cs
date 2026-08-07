using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Documents;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.UI.Completion;

namespace TombLib.Tests;

[TestClass]
public class GameFlowEditorCompletionWindowTests
{
	private static GameFlowLanguageServices CreateLanguageServices()
	{
		var lineService = new GameFlowScriptLineService();
		var documentService = new GameFlowScriptDocumentService(lineService);

		return new GameFlowLanguageServices(
			new GameFlowDefinitionProvider(documentService),
			new GameFlowHoverProvider(),
			new GameFlowCompletionProvider(),
			lineService,
			documentService,
			new GameFlowDocumentLookupService(documentService));
	}

	[TestMethod]
	public void ShowCompletionWindow_ClearsFieldWhenWindowCloses()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow? completionWindow = editor.ActiveCompletionWindow;
				Assert.IsNotNull(completionWindow);
				completionWindow.Close();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				Assert.IsNull(editor.ActiveCompletionWindow);
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
			var editor = new GameFlowEditor(new Version(1, 0), CreateLanguageServices());
			Window hostWindow = WPFTestHelper.ShowInHostWindow(editor);

			try
			{
				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow? firstWindow = editor.ActiveCompletionWindow;
				Assert.IsNotNull(firstWindow);

				editor.InitializeCompletionWindow();
				editor.ShowCompletionWindow();
				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow? secondWindow = editor.ActiveCompletionWindow;

				Assert.AreNotSame(firstWindow, secondWindow);
				Assert.IsFalse(firstWindow.IsVisible);
				Assert.IsTrue(secondWindow.IsVisible);
			}
			finally
			{
				editor.ActiveCompletionWindow?.Close();
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryOpenCompletionWindow_PopulatesItemsAndOffsets()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var editor = new GameFlowEditor(new Version(1, 0), CreateLanguageServices())
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

				CompletionWindow? completionWindow = editor.ActiveCompletionWindow;

				Assert.IsTrue(opened);
				Assert.IsNotNull(completionWindow);
				Assert.AreEqual(1, completionWindow.CompletionList.CompletionData.Count);
				Assert.AreEqual(1, completionWindow.StartOffset);
				Assert.AreEqual(3, completionWindow.EndOffset);
			}
			finally
			{
				editor.ActiveCompletionWindow?.Close();
				hostWindow.Close();
			}
		});
	}
}
