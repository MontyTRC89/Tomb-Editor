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
}
