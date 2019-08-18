using DarkUI.Forms;
using System.ComponentModel;

namespace TombIDE.ScriptEditor
{
	public partial class FormMnemonicInfo : DarkForm
	{
		public FormMnemonicInfo(string flag)
		{
			InitializeComponent();

			richTextBox_Description.Text = GetFlagDescription();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Hide();
			e.Cancel = true;
			base.OnClosing(e);
		}

		private string GetFlagDescription()
		{
			// TODO
			return null;
		}
	}
}
