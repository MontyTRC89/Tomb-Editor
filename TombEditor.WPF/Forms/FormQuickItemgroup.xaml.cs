using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormQuickItemgroup.xaml
/// </summary>
public partial class FormQuickItemgroup : Window
{
	public TombEditor.Forms.FormQuickItemgroup WinFormsLayer { get; }

	public FormQuickItemgroup(Editor editor)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormQuickItemgroup(editor);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
