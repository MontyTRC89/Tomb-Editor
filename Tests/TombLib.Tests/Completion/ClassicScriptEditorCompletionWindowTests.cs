using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Completion;
using TombLib.Scripting.Objects;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptEditorCompletionWindowTests
{
	private static readonly ITextCompletionProvider CompletionProvider = new ClassicScriptCompletionProvider();

	[TestMethod]
	public void EmptyLineCompletion_ItemsExposeKindDetailText()
	{
		IReadOnlyList<TextCompletionItem> completionItems = CompletionProvider.GetCompletionItems(
			new TextCompletionContext(string.Empty, 0, TextCompletionTrigger.EmptyLine));

		Assert.AreEqual("Old Command", completionItems.First(item => item.Kind == TextCompletionItemKind.OldCommand).Detail);
		Assert.AreEqual("New Command", completionItems.First(item => item.Kind == TextCompletionItemKind.NewCommand).Detail);
		Assert.AreEqual("Section", completionItems.First(item => item.Kind == TextCompletionItemKind.Section).Detail);
		Assert.AreEqual("Directive", completionItems.First(item => item.Kind == TextCompletionItemKind.Directive).Detail);
	}

	[TestMethod]
	public void EmptyLineCompletion_OpensCompletionWindowAtLineOffset()
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
				ICompletionData[] completionItems = [..
					CompletionProvider.GetCompletionItems(
						new TextCompletionContext(editor.Text, editor.CaretOffset, TextCompletionTrigger.EmptyLine))
						.Select(item => new CompletionData(item))];
				int lineOffset = editor.Document.GetLineByOffset(editor.CaretOffset).Offset;

				bool opened = (bool)(WPFTestHelper.InvokeInstanceMethod(
					editor,
					"TryOpenCompletionWindow",
					[typeof(IEnumerable<ICompletionData>), typeof(int?), typeof(int?), typeof(int), typeof(int)],
					completionItems,
					lineOffset,
					null,
					300,
					300)
					?? throw new InvalidOperationException("Instance method 'TryOpenCompletionWindow' returned null."));

				WPFTestHelper.PumpDispatcher(editor.Dispatcher, DispatcherPriority.Background);

				CompletionWindow completionWindow = WPFTestHelper.GetPrivateField<CompletionWindow>(editor, "_completionWindow");

				Assert.IsTrue(opened);
				Assert.IsTrue(completionWindow.CompletionList.CompletionData.Count > 0);
				Assert.AreEqual(0, completionWindow.StartOffset);
			}
			finally
			{
				(WPFTestHelper.FindInstanceField(editor.GetType(), "_completionWindow")?.GetValue(editor) as CompletionWindow)?.Close();
				hostWindow.Close();
			}
		});
	}
}
