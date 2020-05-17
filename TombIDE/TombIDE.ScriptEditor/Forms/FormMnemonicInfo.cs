using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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
		OCBs,
		OLDCommands,
		NEWCommands,
		Mnemonics
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
				Text = GetFlagDescription(flag.TrimEnd('='), type),
				ForeColor = Color.Gainsboro,
				BackColor = Color.FromArgb(48, 48, 48),
				Font = new Font("Microsoft Sans Serif", 12f),
				BorderStyle = BorderStyle.None,
				Dock = DockStyle.Fill,
				ReadOnly = true
			};

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

		private string GetFlagDescription(string flag, ReferenceType type)
		{
			string result = string.Empty;

			try
			{
				result = GetDescriptionFromRDDA(flag, type);

				if (string.IsNullOrEmpty(result) && type == ReferenceType.Mnemonics)
					result = GetDescriptionFromPlugin(flag);

				if (!string.IsNullOrEmpty(result))
					result = Regex.Replace(result, @"\r\n?|\n", "\n");
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return result;
		}

		private string GetDescriptionFromRDDA(string flag, ReferenceType type)
		{
			string archivePath = Path.Combine(DefaultPaths.GetReferenceDescriptionsPath(), GetRDDAFileName(type));
			string flagDescriptionFileName = "info_" + flag.TrimStart('_').Replace(" ", "_").Replace("/", string.Empty) + ".txt";

			using (FileStream file = File.OpenRead(archivePath))
			using (ZipArchive archive = new ZipArchive(file))
				foreach (ZipArchiveEntry entry in archive.Entries)
					if (entry.Name.Equals(flagDescriptionFileName, StringComparison.OrdinalIgnoreCase))
						using (Stream stream = entry.Open())
						using (StreamReader reader = new StreamReader(stream))
							return reader.ReadToEnd();

			return string.Empty;
		}

		private string GetDescriptionFromPlugin(string flag)
		{
			foreach (PluginMnemonic mnemonic in ScriptKeywords.PluginMnemonics) // Search for a definition in the plugin mnemonics list
				if (mnemonic.FlagName.Equals(flag, StringComparison.OrdinalIgnoreCase))
					return mnemonic.Description;

			return string.Empty;
		}

		private string GetRDDAFileName(ReferenceType type)
		{
			switch (type)
			{
				case ReferenceType.Mnemonics: return "Mnemonic Constants.rdda";
				case ReferenceType.OLDCommands: return "OLD Commands.rdda";
				case ReferenceType.NEWCommands: return "NEW Commands.rdda";
				case ReferenceType.OCBs: return "OCBs.rdda";
				default: return string.Empty;
			}
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
