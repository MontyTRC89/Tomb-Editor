#nullable enable

using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace TombLib.Scripting.UI.Presentation;

public partial class TextReferencesResultsView : UserControl
{
	private readonly Action<TextReferenceListItem>? _activateReference;

	public TextReferencesResultsView(TextReferencesResultsViewModel viewModel, Action<TextReferenceListItem>? activateReference)
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
		if (ReferencesTree.SelectedItem is not TextReferenceListItem selectedReference)
			return;

		_activateReference?.Invoke(selectedReference);
	}
}
