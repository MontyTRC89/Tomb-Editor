#nullable enable

using DarkUI.Docking;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.ViewModels;
using TombIDE.ScriptingStudio.Views;
using TombIDE.Shared;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.ToolWindows;

public sealed class LuaDiagnostics : DarkToolWindow
{
	private readonly LuaDiagnosticsViewModel _viewModel = new();

	internal LuaDiagnostics(Action<LuaDiagnosticListItem>? activateDiagnostic)
	{
		ElementHost elementHost = new()
		{
			Dock = DockStyle.Fill,
			Child = new LuaDiagnosticsView(_viewModel, activateDiagnostic)
		};

		Controls.Add(elementHost);

		DefaultDockArea = DarkDockArea.Bottom;
		DockText = Strings.Default.LuaDiagnostics;
		Name = nameof(LuaDiagnostics);
		SerializationKey = nameof(LuaDiagnostics);
		Size = new Size(420, 220);
	}

	public void ShowNoActiveDocument()
		=> _viewModel.ShowNoActiveDocument();

	public void ShowPending()
		=> _viewModel.ShowPending();

	public void ShowDiagnostics(string filePath, TextDocument document, IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> _viewModel.ShowDiagnostics(filePath, document, diagnostics);
}