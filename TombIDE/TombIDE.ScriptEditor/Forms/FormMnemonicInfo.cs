using DarkUI.Forms;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.Scripting;

namespace TombIDE.ScriptEditor
{
	public partial class FormMnemonicInfo : DarkForm
	{
		public FormMnemonicInfo(string flag)
		{
			InitializeComponent();

			Text = "Information about " + flag.ToUpper();
			label_FlagName.Text = flag.ToUpper();

			richTextBox_Description.Text = GetFlagDescription(flag);
		}

		protected override void OnShown(EventArgs e)
		{
			if (string.IsNullOrEmpty(richTextBox_Description.Text))
			{
				DarkMessageBox.Show(this, "No description found for the " + label_FlagName.Text + " flag.", "Information",
					MessageBoxButtons.OK, MessageBoxIcon.Information);

				DialogResult = DialogResult.OK;
			}
			else
				base.OnShown(e);
		}

		private string GetFlagDescription(string flag)
		{
			try
			{
				foreach (string file in Directory.GetFiles(Path.Combine(SharedMethods.GetProgramDirectory(), @"References\Mnemonics"), "info_*.txt"))
				{
					if (Path.GetFileName(file).ToLower() == "info_" + flag.ToLower() + ".txt")
						return File.ReadAllText(file, Encoding.GetEncoding(1252));
				}
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			foreach (PluginMnemonic mnemonic in KeyWords.PluginMnemonics)
			{
				if (mnemonic.Flag == flag)
					return mnemonic.Description;
			}

			return string.Empty;
		}
	}
}
