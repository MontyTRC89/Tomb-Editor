using DarkUI.WPF.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombEditor.WPF.ViewModels;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormOptions.xaml
/// </summary>
public partial class FormOptions : Window
{
	public FormOptions(Editor editor)
	{
		this.DataContext = new OptionsViewModel(editor.Configuration);
		InitializeComponent();
	}

	private void ColorSchemePreset_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
	{
    }

	private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
	{
		if (!(sender is ColorPickerButton))
			return;
		var btn = (sender as ColorPickerButton)!;
		using (var colorDialog = new ColorDialog())
		{
			var oldColor = btn.SelectedColor;
			colorDialog.Color = System.Drawing.Color.FromArgb(oldColor.A, oldColor.R, oldColor.G, oldColor.B);
			colorDialog.FullOpen = true;
			var result = colorDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				var pickedColor = colorDialog.Color;
				var newUIColor = System.Windows.Media.Color.FromArgb(pickedColor.A, pickedColor.R, pickedColor.G, pickedColor.B);
				btn.SelectedColor = newUIColor;
				ColorSchemePresetSelection.SelectedIndex = -1; // Make Preset Selection show nothing, because we just changed a color
			}
		}
	}
}
