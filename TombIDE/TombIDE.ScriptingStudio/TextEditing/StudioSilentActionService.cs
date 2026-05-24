#nullable enable

using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.Shared;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.TextEditing;

internal readonly record struct SilentActionFileState(
	string FilePath,
	EditorType EditorType,
	bool OpenSourceView,
	bool WasAlreadyOpen,
	bool WasContentChanged);

internal readonly record struct SilentActionCompletion(
	TabPage? TabPage,
	bool SaveAffectedFile,
	bool CloseAffectedTab);

internal sealed class StudioSilentActionService
{
	private readonly EditorTabControl _editorTabControl;

	public StudioSilentActionService(EditorTabControl editorTabControl)
		=> _editorTabControl = editorTabControl ?? throw new ArgumentNullException(nameof(editorTabControl));

	public TabPage? RememberSelectedTab()
		=> _editorTabControl.SelectedTab;

	public SilentActionFileState CaptureFileState(string filePath, EditorType editorType = EditorType.Default)
	{
		TabPage? tabPage = _editorTabControl.FindTabPage(filePath, editorType);
		bool wasAlreadyOpen = tabPage is not null;
		bool wasContentChanged = wasAlreadyOpen && _editorTabControl.GetEditorOfTab(tabPage) is { } editor && editor.IsContentChanged;

		return new SilentActionFileState(filePath, editorType, false, wasAlreadyOpen, wasContentChanged);
	}

	public SilentActionFileState CaptureSourceFileState(string filePath)
	{
		TabPage? tabPage = _editorTabControl.FindSourceTabPage(filePath);
		bool wasAlreadyOpen = tabPage is not null;
		bool wasContentChanged = wasAlreadyOpen && _editorTabControl.GetEditorOfTab(tabPage) is { } editor && editor.IsContentChanged;

		return new SilentActionFileState(filePath, EditorType.Default, true, wasAlreadyOpen, wasContentChanged);
	}

	public SilentActionCompletion CreateCompletion(
		SilentActionFileState fileState,
		bool saveAffectedFile = true,
		bool closeAffectedTab = true)
	{
		TabPage? tabPage = fileState.OpenSourceView
			? _editorTabControl.FindSourceTabPage(fileState.FilePath)
			: _editorTabControl.FindTabPage(fileState.FilePath, fileState.EditorType);

		return new SilentActionCompletion(
			tabPage,
			saveAffectedFile && !fileState.WasContentChanged,
			closeAffectedTab && !fileState.WasAlreadyOpen);
	}

	public void Complete(TabPage? previousTab, bool indicateChange, params SilentActionCompletion[] completions)
	{
		if (indicateChange && _editorTabControl.CurrentEditor is { } currentEditor)
		{
			currentEditor.LastModified = DateTime.Now;
			IDE.Instance.ScriptEditor_IndicateExternalChange();
		}

		foreach (SilentActionCompletion completion in completions)
		{
			if (completion.SaveAffectedFile && completion.TabPage is not null && _editorTabControl.TabPages.Contains(completion.TabPage))
				_editorTabControl.SaveFile(completion.TabPage);
		}

		foreach (SilentActionCompletion completion in completions)
		{
			if (completion.CloseAffectedTab && completion.TabPage is not null && _editorTabControl.TabPages.Contains(completion.TabPage))
				_editorTabControl.TabPages.Remove(completion.TabPage);
		}

		_editorTabControl.EnsureTabFileSynchronization();

		if (previousTab is not null && _editorTabControl.TabPages.Contains(previousTab))
			_editorTabControl.SelectTab(previousTab);
	}
}