using ICSharpCode.AvalonEdit.Document;
using Moq;
using Nickelony.LanguageServer.Abstractions.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Editing;
using TombLib.Scripting.Lua;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class TextWorkspaceEditApplierTests
{
	[TestMethod]
	public void Apply_MultipleFilesSynchronizesOpenEditorsAndReplaysSnapshots()
	{
		StaTestHelper.RunInSta(() =>
		{
			var firstEditor = CreateEditor(@"C:\Scripts\first.lua", "first");
			var firstMirror = CreateEditor(@"C:\Scripts\first.lua", "first");
			var secondEditor = CreateEditor(@"C:\Scripts\second.lua", "second");
			var host = new TestEditorHost(firstEditor, firstMirror, secondEditor);
			var applier = new TextWorkspaceEditApplier(host);

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[
						new TextDocumentEdit(@"C:\Scripts\first.lua", [CreateEdit(1, 1, 1, 6, "changed")]),
						new TextDocumentEdit(@"C:\Scripts\second.lua", [CreateEdit(1, 1, 1, 7, "updated")])
					]);

				TextWorkspaceEditTransaction transaction = applier.Apply(workspaceEdit);

				Assert.IsTrue(transaction.HasChanges);
				Assert.AreEqual("changed", firstEditor.Text);
				Assert.AreEqual("changed", firstMirror.Text);
				Assert.AreEqual("updated", secondEditor.Text);

				applier.ApplyBeforeSnapshot(transaction);
				Assert.AreEqual("first", firstEditor.Text);
				Assert.AreEqual("first", firstMirror.Text);
				Assert.AreEqual("second", secondEditor.Text);

				applier.ApplyAfterSnapshot(transaction);
				Assert.AreEqual("changed", firstEditor.Text);
				Assert.AreEqual("changed", firstMirror.Text);
				Assert.AreEqual("updated", secondEditor.Text);
			}
			finally
			{
				firstEditor.Dispose();
				firstMirror.Dispose();
				secondEditor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_InvalidRange_RejectsBeforeMutatingAnyFile()
	{
		StaTestHelper.RunInSta(() =>
		{
			var firstEditor = CreateEditor(@"C:\Scripts\first.lua", "first");
			var secondEditor = CreateEditor(@"C:\Scripts\second.lua", "second");
			var host = new TestEditorHost(firstEditor, secondEditor);
			var applier = new TextWorkspaceEditApplier(host);

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[
						new TextDocumentEdit(@"C:\Scripts\first.lua", [CreateEdit(1, 1, 1, 6, "changed")]),
						new TextDocumentEdit(@"C:\Scripts\second.lua", [CreateEdit(2, 1, 2, 2, "invalid")])
					]);

				Assert.ThrowsException<InvalidOperationException>(() => applier.Apply(workspaceEdit));
				Assert.AreEqual("changed", firstEditor.Text);
				Assert.AreEqual("second", secondEditor.Text);
			}
			finally
			{
				firstEditor.Dispose();
				secondEditor.Dispose();
			}
		});
	}

	[TestMethod]
	public void Apply_OverlappingEdits_UsesDeterministicDescendingOffsetOrder()
	{
		StaTestHelper.RunInSta(() =>
		{
			var editor = CreateEditor(@"C:\Scripts\overlap.lua", "abcdef");
			var host = new TestEditorHost(editor);
			var applier = new TextWorkspaceEditApplier(host);

			try
			{
				var workspaceEdit = new TextWorkspaceEdit(
					[
						new TextDocumentEdit(@"C:\Scripts\overlap.lua", [
							CreateEdit(1, 2, 1, 5, "X"),
							CreateEdit(1, 3, 1, 4, "Y")
						])
					]);

				TextWorkspaceEditTransaction transaction = applier.Apply(workspaceEdit);

				Assert.AreEqual("aXef", editor.Text);
				Assert.AreEqual("abcdef", transaction.DocumentChanges[0].BeforeContent);
				Assert.AreEqual("aXef", transaction.DocumentChanges[0].AfterContent);
			}
			finally
			{
				editor.Dispose();
			}
		});
	}

	private static LuaEditor CreateEditor(string filePath, string content)
		=> new(new Version(1, 0)) { FilePath = filePath, Content = content };

	private static TextEdit CreateEdit(int startLine, int startColumn, int endLine, int endColumn, string newText)
		=> new(new TextDocumentRange(startLine, startColumn, endLine, endColumn), newText);

	private sealed class TestEditorHost(params LuaEditor[] editors) : ITextEditorHost
	{
		private readonly IReadOnlyList<LuaEditor> _editors = editors;

		public TextEditorBase OpenTextEditor(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false)
			=> _editors.First(editor => string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

		public IReadOnlyList<IEditorControl> GetOpenEditors(string filePath)
			=> _editors
				.Where(editor => string.Equals(editor.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
				.Cast<IEditorControl>()
				.ToArray();

		public TResult ExecutePreservingSelection<TResult>(Func<TResult> action)
			=> action();
	}
}