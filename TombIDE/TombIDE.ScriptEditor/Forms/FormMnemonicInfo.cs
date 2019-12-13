using DarkUI.Forms;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
            var result = string.Empty;

            try
            {
                var mnemonicPath = Path.Combine(SharedMethods.GetProgramDirectory(), @"References\Mnemonics\info_" + flag.ToLower() + ".txt");
                if (File.Exists(mnemonicPath))
                    result = File.ReadAllText(mnemonicPath, Encoding.GetEncoding(1252));
                else
                {
                    foreach (PluginMnemonic mnemonic in KeyWords.PluginMnemonics)
                        if (mnemonic.Flag == flag)
                            result = mnemonic.Description;
                }

                // Fix Nickelony's persistent problems with line endings
                if (!string.IsNullOrEmpty(result))
                    result = Regex.Replace(result, @"\r\n?|\n", "\n");
            }
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return result;
        }
	}
}
