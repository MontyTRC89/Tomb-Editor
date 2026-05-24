#nullable enable

using DarkUI.Docking;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Scripting.Diagnostics;
using TombLib.Scripting.UI.Presentation;

namespace TombIDE.ScriptingStudio.ToolWindows;

public sealed class TextDiagnosticsToolWindow : DarkToolWindow
{
	private readonly TextDiagnosticsViewModel _viewModel;

	internal TextDiagnosticsToolWindow(
		string dockText,
		string serializationKey,
		TextDiagnosticsPresentation presentation,
		Action<TextDiagnosticListItem>? activateDiagnostic)
	{
		_viewModel = new TextDiagnosticsViewModel(presentation);

		ElementHost elementHost = new()
		{
			Dock = DockStyle.Fill,
			Child = new TextDiagnosticsView(_viewModel, activateDiagnostic)
		};

		Controls.Add(elementHost);

		DefaultDockArea = DarkDockArea.Bottom;
		DockText = dockText;
		Name = serializationKey;
		SerializationKey = serializationKey;
		Size = new Size(420, 220);
	}

	public void ShowNoActiveDocument()
		=> _viewModel.ShowNoActiveDocument();

	public void ShowPending()
		=> _viewModel.ShowPending();

	public void ShowDiagnostics(string filePath, TextDocument document, IReadOnlyList<TextEditorDiagnostic> diagnostics)
		=> _viewModel.ShowDiagnostics(filePath, document, diagnostics);
}