#nullable enable

using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.ToolWindows;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	public LuaDiagnostics LuaDiagnostics = null!;

	private void InitializeLuaDiagnostics()
	{
		LuaDiagnostics = new LuaDiagnostics(NavigateToDiagnostic);
	}

	private void EditorTabControl_LuaSelectedIndexChanged(object? sender, EventArgs e)
	{
		RefreshLuaDiagnosticsView();
		UpdateLuaFeatureCommandAvailability();
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
			diagnostics ?? _intellisenseProvider.GetDiagnostics(editor.FilePath));
	}

	private void NavigateToDiagnostic(LuaDiagnosticListItem diagnostic)
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