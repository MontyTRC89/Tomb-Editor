using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Utils;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormOperationDialog.xaml
/// </summary>
public partial class FormOperationDialog : Window
{
	public TombEditor.Forms.FormOperationDialog WinFormsLayer { get; }

	public FormOperationDialog(string operationName, bool autoCloseWhenDone, bool noProgressBar, Action<IProgressReporter> operation)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormOperationDialog(operationName, autoCloseWhenDone, noProgressBar, operation);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
