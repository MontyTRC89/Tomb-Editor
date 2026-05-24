#nullable enable

using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Scripting.UI.Presentation;

namespace TombIDE.ScriptingStudio.ToolWindows;

public sealed class TextReferencesResultsToolWindow : DarkToolWindow
{
	private readonly TextReferencesResultsViewModel _viewModel;

	internal TextReferencesResultsToolWindow(
		string dockText,
		string serializationKey,
		TextReferencesPresentation presentation,
		Action<TextReferenceListItem>? activateReference)
	{
		_viewModel = new TextReferencesResultsViewModel(presentation);

		ElementHost elementHost = new()
		{
			Dock = DockStyle.Fill,
			Child = new TextReferencesResultsView(_viewModel, activateReference)
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

	public void ShowUnsupported()
		=> _viewModel.ShowUnsupported();

	public void ShowLoading()
		=> _viewModel.ShowLoading();

	internal void ShowReferences(IReadOnlyList<TextReferenceGroup> groups)
		=> _viewModel.ShowReferences(groups);
}