#nullable enable

using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.ScriptingStudio.Controls;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.TextEditing;

internal sealed class DocumentControllerTextEditorHost : ITextEditorHost
{
	private readonly IEditorDocumentController _documentController;

	public DocumentControllerTextEditorHost(IEditorDocumentController documentController)
	{
		_documentController = documentController ?? throw new ArgumentNullException(nameof(documentController));
	}

	public TEditor? OpenEditor<TEditor>(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false)
		where TEditor : class, IEditorControl
		=> OpenEditorControl(filePath, editorType, openSourceView) as TEditor;

	public TextDocument? TryGetTextDocument(string filePath)
	{
		if (_documentController.FindEditorsOfFile(filePath)
			.OfType<TextEditorBase>()
			.FirstOrDefault() is { } openEditor)
		{
			return openEditor.Document;
		}

		if (!File.Exists(filePath))
			return null;

		return new TextDocument(File.ReadAllText(filePath));
	}

	public TextEditorBase OpenTextEditor(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false)
	{
		if (!_documentController.FindEditorsOfFile(filePath).Any() && !File.Exists(filePath))
			throw new FileNotFoundException("Unable to apply a workspace edit because the target file could not be found.", filePath);

		IEditorControl editor = OpenEditorControl(filePath, editorType, openSourceView);

		if (editor is not TextEditorBase textEditor)
			throw new InvalidOperationException($"Unable to apply workspace edits to '{filePath}'.");

		return textEditor;
	}

	public IReadOnlyList<IEditorControl> GetOpenEditors(string filePath)
		=> [.. _documentController.FindEditorsOfFile(filePath)];

	public TResult ExecutePreservingSelection<TResult>(Func<TResult> action)
	{
		ArgumentNullException.ThrowIfNull(action);

		IEditorControl? previouslySelectedEditor = _documentController.CurrentEditor;

		try
		{
			return action();
		}
		finally
		{
			if (previouslySelectedEditor is not null && _documentController.ContainsEditor(previouslySelectedEditor))
				_documentController.ActivateEditor(previouslySelectedEditor);
		}
	}

	private IEditorControl OpenEditorControl(string filePath, EditorType editorType, bool openSourceView)
	{
		if (openSourceView)
			_documentController.OpenSourceFile(filePath);
		else
			_documentController.OpenFile(filePath, editorType);

		return _documentController.CurrentEditor
			?? throw new InvalidOperationException($"Unable to open '{filePath}'.");
	}
}
