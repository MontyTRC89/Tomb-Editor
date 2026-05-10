#nullable enable

using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.ViewModels;
using TombIDE.ScriptingStudio.Views;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.ToolWindows;

public sealed class LuaReferencesResults : DarkToolWindow
{
	private readonly LuaReferencesResultsViewModel _viewModel = new();

	internal LuaReferencesResults(Action<LuaReferenceListItem>? activateReference)
	{
		ElementHost elementHost = new()
		{
			Dock = DockStyle.Fill,
			Child = new LuaReferencesResultsView(_viewModel, activateReference)
		};

		Controls.Add(elementHost);

		DefaultDockArea = DarkDockArea.Bottom;
		DockText = Strings.Default.LuaReferencesResults;
		Name = nameof(LuaReferencesResults);
		SerializationKey = nameof(LuaReferencesResults);
		Size = new Size(420, 220);
	}

	public void ShowNoActiveDocument()
		=> _viewModel.ShowNoActiveDocument();

	public void ShowUnsupported()
		=> _viewModel.ShowUnsupported();

	public void ShowLoading()
		=> _viewModel.ShowLoading();

	internal void ShowReferences(IReadOnlyList<LuaReferenceGroup> groups)
		=> _viewModel.ShowReferences(groups);
}