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
		public FormMnemonicInfo(string flag, bool isOCB = false)
		{
			InitializeComponent();

			Text = "Information about " + flag.ToUpper();
			label_FlagName.Text = flag.ToUpper();

			richTextBox_Description.Text = isOCB ? GetOCBDescription(flag) : GetFlagDescription(flag);
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
			string result = string.Empty;

			try
			{
				string mnemonicPath = Path.Combine(SharedMethods.GetProgramDirectory(),
					@"References\Mnemonics\info_" + flag.ToLower() + ".txt");

				if (File.Exists(mnemonicPath))
					result = File.ReadAllText(mnemonicPath, Encoding.GetEncoding(1252));
				else
				{
					foreach (PluginMnemonic mnemonic in KeyWords.PluginMnemonics)
					{
						if (mnemonic.Flag == flag)
							result = mnemonic.Description;
					}
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

		private string GetOCBDescription(string flag)
		{
			string result = string.Empty;

			try
			{
				string ocbPath = Path.Combine(SharedMethods.GetProgramDirectory(),
					@"References\OCBs\info_" + flag.TrimStart('_').Replace(" ", "_").Replace("/", string.Empty).ToLower() + ".txt");

				if (File.Exists(ocbPath))
					result = File.ReadAllText(ocbPath, Encoding.GetEncoding(1252));

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
