using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Resources;

namespace TombIDE.ScriptEditor.Forms
{
	// TODO: Refactor !!!

	internal enum ReferenceType
	{
		OCB,
		OLDCommands,
		NEWCommands,
		Other
	}

	internal partial class FormMnemonicInfo : DarkForm
	{
		private IDE _ide;

		#region Construction and public methods

		public FormMnemonicInfo(IDE ide)
		{
			InitializeComponent();

			_ide = ide;

			checkBox_AlwaysTop.Checked = _ide.IDEConfiguration.InfoBox_AlwaysOnTop;
			checkBox_CloseTabs.Checked = _ide.IDEConfiguration.InfoBox_CloseTabsOnClose;
		}

		public void Show(string flag, ReferenceType type)
		{
			if (!Visible)
				Show();

			OpenDescriptionFile(flag, type);

			Focus();
		}

		#endregion Construction and public methods

		#region Events

		protected override void OnClosing(CancelEventArgs e)
		{
			if (_ide.IDEConfiguration.InfoBox_CloseTabsOnClose)
				tabControl.TabPages.Clear();

			Hide();
			e.Cancel = true; // This form should never be closed during runtime

			base.OnClosing(e);
		}

		private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControl.SelectedTab == null)
				Hide();
			else
				Text = "Information about " + tabControl.SelectedTab.Text;
		}

		private void tabControl_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Middle)
				HandleMiddleMouseTabClosing(e);
		}

		private void checkBox_AlwaysTop_CheckedChanged(object sender, EventArgs e)
		{
			_ide.IDEConfiguration.InfoBox_AlwaysOnTop = checkBox_AlwaysTop.Checked;

			TopMost = _ide.IDEConfiguration.InfoBox_AlwaysOnTop;
		}

		private void checkBox_CloseTabs_CheckedChanged(object sender, EventArgs e) =>
			_ide.IDEConfiguration.InfoBox_CloseTabsOnClose = checkBox_CloseTabs.Checked;

		#endregion Events

		#region Methods

		private void OpenDescriptionFile(string flag, ReferenceType type) // TODO: Refactor
		{
			foreach (TabPage tab in tabControl.TabPages)
				if (tab.Text.Equals(flag, StringComparison.OrdinalIgnoreCase))
				{
					tabControl.SelectTab(tab);
					return;
				}

			TabPage newTabPage = new TabPage(flag.ToUpper())
			{
				UseVisualStyleBackColor = false,
				BackColor = Color.FromArgb(42, 42, 42),
				Size = tabControl.Size,
				Padding = new Padding(5)
			};

			RichTextBox textBox = new RichTextBox
			{
				ForeColor = Color.Gainsboro,
				BackColor = Color.FromArgb(48, 48, 48),
				Font = new Font("Microsoft Sans Serif", 12f),
				BorderStyle = BorderStyle.None,
				Dock = DockStyle.Fill,
				ReadOnly = true
			};

			switch (type)
			{
				case ReferenceType.OCB:
					textBox.Text = GetOCBDescription(flag);
					break;

				case ReferenceType.OLDCommands:
					textBox.Text = GetCommandDescription(flag.TrimEnd('='), type);
					break;

				case ReferenceType.NEWCommands:
					textBox.Text = GetCommandDescription(flag.TrimEnd('='), type);
					break;

				default:
					textBox.Text = GetFlagDescription(flag);
					break;
			}

			newTabPage.Controls.Add(textBox);
			tabControl.TabPages.Add(newTabPage);

			tabControl.SelectTab(newTabPage);

			Text = "Information about " + tabControl.SelectedTab.Text;

			if (string.IsNullOrEmpty(textBox.Text))
			{
				DarkMessageBox.Show(this, "No description found for the " + flag.ToUpper() + " flag.", "Information",
					MessageBoxButtons.OK, MessageBoxIcon.Information);

				newTabPage.Dispose();
			}

			if (tabControl.TabPages.Count == 0)
				Hide();
		}

		private string GetCommandDescription(string flag, ReferenceType type)
		{
			string result = string.Empty;

			try
			{
				string filePath = Path.Combine(PathHelper.GetMnemonicDefinitionsPath(), "info_" + flag + ".txt");

				switch (type)
				{
					case ReferenceType.OLDCommands:
						filePath = Path.Combine(PathHelper.GetOLDCommandDefinitionsPath(), "info_" + flag + ".txt");
						break;

					case ReferenceType.NEWCommands:
						filePath = Path.Combine(PathHelper.GetNEWCommandDefinitionsPath(), "info_" + flag + ".txt");
						break;

					default:
						return null;
				}

				if (File.Exists(filePath))
					result = File.ReadAllText(filePath, Encoding.GetEncoding(1252));

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

		private string GetFlagDescription(string flag)
		{
			string result = string.Empty;

			try
			{
				string mnemonicPath = Path.Combine(PathHelper.GetMnemonicDefinitionsPath(), "info_" + flag + ".txt");

				if (File.Exists(mnemonicPath))
					result = File.ReadAllText(mnemonicPath, Encoding.GetEncoding(1252));
				else
					foreach (PluginMnemonic mnemonic in ScriptKeyWords.PluginMnemonics) // Search for a definition in the plugin mnemonics list
						if (mnemonic.FlagName == flag)
							result = mnemonic.Description;

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
				string ocbPath = Path.Combine(PathHelper.GetOCBDefinitionsPath(),
					"info_" + flag.TrimStart('_').Replace(" ", "_").Replace("/", string.Empty) + ".txt");

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

		private void HandleMiddleMouseTabClosing(MouseEventArgs e)
		{
			for (int i = 0; i < tabControl.TabPages.Count; i++) // Check which tab page was middle-clicked
				if (tabControl.GetTabRect(i).Contains(e.Location))
				{
					tabControl.TabPages[i].Dispose();
					return;
				}
		}

		#endregion Methods
	}
}
