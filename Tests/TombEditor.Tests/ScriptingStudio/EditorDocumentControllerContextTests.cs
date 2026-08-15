using System;
using System.IO;
using Moq;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.ScriptingStudio.Editors;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public sealed class EditorDocumentControllerContextTests
{
	[TestMethod]
	public void CurrentDocumentContext_TracksRegistrationAndMonotonicTransitions()
	{
		using var firstFile = new TemporaryFile("first.txt");
		using var secondFile = new TemporaryFile("second.lua");
		var controller = new EditorDocumentController(new Version(1, 0), string.Empty);
		IEditorControl firstEditor = CreateEditor(EditorType.Text);
		IEditorControl secondEditor = CreateEditor(EditorType.Text);
		controller.RegisterDocument(CreateRegistration(EditorType.Text, DocumentMode.ClassicScript, _ => true, filePath => filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase), firstEditor));
		controller.RegisterDocument(CreateRegistration(EditorType.Text, DocumentMode.Lua, _ => true, _ => true, secondEditor));

		var contexts = new List<ScriptingDocumentContext>();
		controller.CurrentEditorChanged += (_, args) => contexts.Add(args.Context);

		Assert.IsFalse(controller.CurrentDocumentContext.HasDocument);

		controller.OpenFile(firstFile.Path);
		ScriptingDocumentContext firstContext = controller.CurrentDocumentContext;
		controller.OpenFile(secondFile.Path);
		ScriptingDocumentContext secondContext = controller.CurrentDocumentContext;

		Assert.AreSame(firstEditor, firstContext.Editor);
		Assert.AreEqual(firstFile.Path, firstContext.FilePath);
		Assert.AreEqual(DocumentMode.ClassicScript, firstContext.Registration!.DocumentMode);
		Assert.AreSame(secondEditor, secondContext.Editor);
		Assert.AreEqual(secondFile.Path, secondContext.FilePath);
		Assert.AreEqual(DocumentMode.Lua, secondContext.Registration!.DocumentMode);
		Assert.IsTrue(secondContext.Generation > firstContext.Generation);
		Assert.AreEqual(2, contexts.Count);

		controller.ActivateEditor(secondEditor);
		controller.ActivateEditor(firstEditor);

		Assert.AreEqual(3, contexts.Count);
		Assert.AreSame(firstEditor, controller.CurrentDocumentContext.Editor);
		Assert.AreEqual(DocumentMode.ClassicScript, controller.CurrentDocumentContext.Registration!.DocumentMode);
		Assert.IsTrue(controller.CurrentDocumentContext.Generation > secondContext.Generation);
	}

	[TestMethod]
	public void ClosingCurrentEditor_TransitionsToReplacementThenExplicitEmptyContext()
	{
		using var firstFile = new TemporaryFile("first.txt");
		using var secondFile = new TemporaryFile("second.txt");
		var controller = new EditorDocumentController(new Version(1, 0), string.Empty);
		IEditorControl firstEditor = CreateEditor(EditorType.Text);
		IEditorControl secondEditor = CreateEditor(EditorType.Text);
		controller.RegisterDocument(CreateRegistration(EditorType.Text, DocumentMode.ClassicScript, _ => true, filePath => filePath == firstFile.Path, firstEditor));
		controller.RegisterDocument(CreateRegistration(EditorType.Text, DocumentMode.Lua, _ => true, filePath => filePath == secondFile.Path, secondEditor));

		var contexts = new List<ScriptingDocumentContext>();
		controller.CurrentEditorChanged += (_, args) => contexts.Add(args.Context);

		controller.OpenFile(firstFile.Path);
		controller.OpenFile(secondFile.Path);
		long secondGeneration = controller.CurrentDocumentContext.Generation;

		Assert.IsTrue(controller.TryCloseEditor(secondEditor));
		ScriptingDocumentContext replacementContext = controller.CurrentDocumentContext;

		Assert.AreSame(firstEditor, replacementContext.Editor);
		Assert.IsTrue(replacementContext.Generation > secondGeneration);

		Assert.IsTrue(controller.TryCloseEditor(firstEditor));
		ScriptingDocumentContext emptyContext = controller.CurrentDocumentContext;

		Assert.IsFalse(emptyContext.HasDocument);
		Assert.IsNull(emptyContext.Editor);
		Assert.IsNull(emptyContext.Registration);
		Assert.IsTrue(emptyContext.Generation > replacementContext.Generation);
		Assert.AreEqual(4, contexts.Count);
	}

	[TestMethod]
	public void RapidMixedLanguageTransitions_RestoreExactRegistrationWithNewGeneration()
	{
		using var luaFile = new TemporaryFile("script.lua");
		using var primaryFile = new TemporaryFile("script.txt");
		var controller = new EditorDocumentController(new Version(1, 0), string.Empty);
		IEditorControl luaEditor = CreateEditor(EditorType.Text);
		IEditorControl primaryEditor = CreateEditor(EditorType.Text);
		controller.RegisterDocument(CreateRegistration(EditorType.Text, DocumentMode.ClassicScript, _ => true, filePath => filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase), primaryEditor));
		controller.RegisterDocument(CreateRegistration(EditorType.Text, DocumentMode.Lua, _ => true, _ => true, luaEditor));

		controller.OpenFile(luaFile.Path);
		ScriptingDocumentContext firstLuaContext = controller.CurrentDocumentContext;
		controller.OpenFile(primaryFile.Path);
		ScriptingDocumentContext primaryContext = controller.CurrentDocumentContext;
		controller.ActivateEditor(luaEditor);
		ScriptingDocumentContext secondLuaContext = controller.CurrentDocumentContext;

		Assert.AreEqual(DocumentMode.Lua, firstLuaContext.Registration!.DocumentMode);
		Assert.AreEqual(DocumentMode.ClassicScript, primaryContext.Registration!.DocumentMode);
		Assert.AreEqual(DocumentMode.Lua, secondLuaContext.Registration!.DocumentMode);
		Assert.AreSame(luaEditor, secondLuaContext.Editor);
		Assert.IsTrue(primaryContext.Generation > firstLuaContext.Generation);
		Assert.IsTrue(secondLuaContext.Generation > primaryContext.Generation);
	}

	private static IEditorControl CreateEditor(EditorType editorType)
	{
		var mock = new Mock<IEditorControl>();
		mock.SetupGet(editor => editor.EditorType).Returns(editorType);
		mock.SetupProperty(editor => editor.FilePath);
		mock.Setup(editor => editor.Load(It.IsAny<string>(), It.IsAny<bool>()))
			.Callback((string filePath, bool _) => mock.Object.FilePath = filePath);
		mock.SetupProperty(editor => editor.Content);
		mock.SetupProperty(editor => editor.IsContentChanged);
		mock.SetupProperty(editor => editor.LastModified);
		return mock.Object;
	}

	private static ScriptingDocumentRegistration CreateRegistration(
		EditorType editorType,
		DocumentMode documentMode,
		Func<string, bool> supportsFile,
		Func<string, bool> isDefaultForFile,
		IEditorControl editor)
		=> new(editorType, documentMode, supportsFile, isDefaultForFile, _ => editor, ScriptingDocumentContributions.None);

	private sealed class TemporaryFile : IDisposable
	{
		public TemporaryFile(string fileName)
		{
			Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"TombEditor-{Guid.NewGuid():N}-{fileName}");
			File.WriteAllText(Path, string.Empty);
		}

		public string Path { get; }

		public void Dispose()
		{
			if (File.Exists(Path))
				File.Delete(Path);
		}
	}
}