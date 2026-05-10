#nullable enable

using System;
using System.Windows.Controls;
using System.Windows.Input;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.ViewModels;

namespace TombIDE.ScriptingStudio.Views;

public partial class LuaReferencesResultsView : UserControl
{
	private readonly Action<LuaReferenceListItem>? _activateReference;

	internal LuaReferencesResultsView(LuaReferencesResultsViewModel viewModel, Action<LuaReferenceListItem>? activateReference)
	{
		InitializeComponent();
		DataContext = viewModel;
		_activateReference = activateReference;
	}

	private void ReferencesTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		=> ActivateSelectedReference();

	private void ReferencesTree_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter)
			return;

		ActivateSelectedReference();
		e.Handled = true;
	}

	private void ActivateSelectedReference()
	{
		if (ReferencesTree.SelectedItem is not LuaReferenceListItem selectedReference)
			return;

		_activateReference?.Invoke(selectedReference);
	}
}