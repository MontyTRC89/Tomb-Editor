using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for FormToolBarLayout.xaml
/// </summary>
public partial class FormToolBarLayout : Window
{
	public TombEditor.Forms.FormToolBarLayout WinFormsLayer { get; }

	public FormToolBarLayout(Editor editor, List<ToolStripItem> toolstripButtons)
	{
		InitializeComponent();
		WinFormsLayer = new TombEditor.Forms.FormToolBarLayout(editor, toolstripButtons);

		var panel = new Panel();
		panel.Controls.AddRange(WinFormsLayer.Controls.Cast<Control>().ToArray());
		panel.Dock = DockStyle.Fill;

		Content = new WindowsFormsHost { Child = panel };
	}
}
