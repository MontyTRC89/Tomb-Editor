using DarkUI.Forms;
using System.ComponentModel;

namespace TombIDE.ScriptEditor
{
	public partial class FormMnemonicInfo : DarkForm
	{
		public FormMnemonicInfo()
		{
			InitializeComponent();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Hide();
			e.Cancel = true;
			base.OnClosing(e);
		}
	}
}
