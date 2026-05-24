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

internal sealed class EditorTabControlTextEditorHost : ITextEditorHost
{
	private readonly EditorTabControl _editorTabControl;

	public EditorTabControlTextEditorHost(EditorTabControl editorTabControl)
	{
		_editorTabControl = editorTabControl ?? throw new ArgumentNullException(nameof(editorTabControl));
	}

	public TEditor? OpenEditor<TEditor>(string filePath, EditorType editorType = EditorType.Default, bool openSourceView = false)
		where TEditor : class, IEditorControl
		=> OpenEditorControl(filePath, editorType, openSourceView) as TEditor;

	public TextDocument? TryGetTextDocument(string filePath)
	{
		if (_editorTabControl.FindTabPagesOfFile(filePath)
			.Select(_editorTabControl.GetEditorOfTab)
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
		if (!_editorTabControl.FindTabPagesOfFile(filePath).Any() && !File.Exists(filePath))
			throw new FileNotFoundException("Unable to apply a workspace edit because the target file could not be found.", filePath);

		IEditorControl editor = OpenEditorControl(filePath, editorType, openSourceView);

		if (editor is not TextEditorBase textEditor)
			throw new InvalidOperationException($"Unable to apply workspace edits to '{filePath}'.");

		return textEditor;
	}

	public IReadOnlyList<IEditorControl> GetOpenEditors(string filePath)
		=> [..
			from tabPage in _editorTabControl.FindTabPagesOfFile(filePath)
			let editor = _editorTabControl.GetEditorOfTab(tabPage)
			where editor is not null
			select editor];

	public TResult ExecutePreservingSelection<TResult>(Func<TResult> action)
	{
		ArgumentNullException.ThrowIfNull(action);

		var previouslySelectedTab = _editorTabControl.SelectedTab;

		try
		{
			return action();
		}
		finally
		{
			if (previouslySelectedTab is not null && _editorTabControl.TabPages.Contains(previouslySelectedTab))
				_editorTabControl.SelectTab(previouslySelectedTab);
		}
	}

	private IEditorControl OpenEditorControl(string filePath, EditorType editorType, bool openSourceView)
	{
		if (openSourceView)
			_editorTabControl.OpenSourceFile(filePath);
		else
			_editorTabControl.OpenFile(filePath, editorType);

		return _editorTabControl.CurrentEditor
			?? throw new InvalidOperationException($"Unable to open '{filePath}'.");
	}
}