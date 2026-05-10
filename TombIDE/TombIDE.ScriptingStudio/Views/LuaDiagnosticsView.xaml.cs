#nullable enable

using System;
using System.Windows.Controls;
using System.Windows.Input;
using TombIDE.ScriptingStudio.Objects;
using TombIDE.ScriptingStudio.ViewModels;

namespace TombIDE.ScriptingStudio.Views;

public partial class LuaDiagnosticsView : UserControl
{
	private readonly Action<LuaDiagnosticListItem>? _activateDiagnostic;

	internal LuaDiagnosticsView(LuaDiagnosticsViewModel viewModel, Action<LuaDiagnosticListItem>? activateDiagnostic)
	{
		InitializeComponent();
		DataContext = viewModel;
		_activateDiagnostic = activateDiagnostic;
	}

	private LuaDiagnosticsViewModel ViewModel => (LuaDiagnosticsViewModel)DataContext;

	private void DiagnosticsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		=> ActivateSelectedDiagnostic();

	private void DiagnosticsGrid_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter)
			return;

		ActivateSelectedDiagnostic();
		e.Handled = true;
	}

	private void ActivateSelectedDiagnostic()
	{
		if (ViewModel.SelectedItem is not LuaDiagnosticListItem selectedDiagnostic)
			return;

		_activateDiagnostic?.Invoke(selectedDiagnostic);
	}
}