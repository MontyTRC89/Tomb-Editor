using DarkUI.Forms;
using System;
using System.IO;
using System.Windows.Forms;

namespace ScriptEditor
{
	public partial class ReferenceBrowser : UserControl
	{
		public ReferenceBrowser()
		{
			InitializeComponent();
		}

		private void ReferenceBrowser_Invalidated(object sender, EventArgs e)
		{
			refSelectionComboBox.SelectedIndex = 0;
		}

		private void refSelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				refTextBox.Text = File.ReadAllText(@"References\" + refSelectionComboBox.Text + ".txt");
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void refSearchTextBox_GotFocus(object sender, EventArgs e)
		{
			if (refSearchTextBox.Text == "Search references...")
			{
				refSearchTextBox.Text = string.Empty;
			}
		}

		private void refSearchTextBox_LostFocus(object sender, EventArgs e)
		{
			if (refSearchTextBox.Text == string.Empty)
			{
				refSearchTextBox.Text = "Search references...";
			}
		}
	}
}
