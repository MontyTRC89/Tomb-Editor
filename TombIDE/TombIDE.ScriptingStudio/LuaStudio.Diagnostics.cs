#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Navigation;
using TombIDE.ScriptingStudio.ToolWindows;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Presentation;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private TextDiagnosticsToolWindow LuaDiagnostics
		=> GetPaneContent<TextDiagnosticsToolWindow>(UICommand.LuaDiagnostics);

	private TextDiagnosticsToolWindow CreateLuaDiagnosticsToolWindow()
		=> new TextDiagnosticsToolWindow(
			Shared.Strings.Default.LuaDiagnostics,
			nameof(LuaDiagnostics),
			new TextDiagnosticsPresentation(
				Shared.Strings.Default.LuaDiagnosticsNoDocument,
				Shared.Strings.Default.LuaDiagnosticsUpdating,
				Shared.Strings.Default.NoDiagnostics,
				Shared.Strings.Default.Errors,
				Shared.Strings.Default.Warnings,
				Shared.Strings.Default.Messages,
				Shared.Strings.Default.Severity,
				Shared.Strings.Default.LineHeader,
				Shared.Strings.Default.ColumnHeader,
				Shared.Strings.Default.Message),
			NavigateToDiagnostic);

	private void EditorTabControl_LuaSelectedIndexChanged(object? sender, EventArgs e)
	{
		RefreshLuaDiagnosticsView();
		UpdateDocumentCommandStates();
	}

	private void LuaEditor_TextChanged(object? sender, EventArgs e)
	{
		InvalidateWorkspaceEditHistory();

		if (!ReferenceEquals(sender, CurrentEditor))
			return;

		RefreshLuaDiagnosticsView(isPending: true);
	}

	private void RefreshLuaDiagnosticsView(bool isPending = false, IReadOnlyList<TextEditorDiagnostic>? diagnostics = null)
	{
		if (CurrentEditor is not LuaEditor editor)
		{
			LuaDiagnostics.ShowNoActiveDocument();
			return;
		}

		if (isPending)
		{
			LuaDiagnostics.ShowPending();
			return;
		}

		LuaDiagnostics.ShowDiagnostics(
			editor.FilePath,
			editor.Document,
			diagnostics ?? _trackedDocumentStateService.GetDiagnostics(editor.FilePath));
	}

	private void NavigateToDiagnostic(TextDiagnosticListItem diagnostic)
		=> NavigateToLocation(
			diagnostic.FilePath,
			NavigationOrigin.Diagnostics,
			_ => new EditorNavigationLocation(
				diagnostic.FilePath,
				diagnostic.StartOffset,
				diagnostic.StartOffset,
				Math.Max(0, diagnostic.EndOffset - diagnostic.StartOffset),
				diagnostic.LineNumber));
}