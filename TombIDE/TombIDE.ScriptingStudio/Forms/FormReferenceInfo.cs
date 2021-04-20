using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Utils;

namespace TombIDE.ScriptingStudio.Forms
{
	// TODO: Refactor !!!

	public partial class FormReferenceInfo : DarkForm
	{
		#region Construction and public methods

		public FormReferenceInfo()
		{
			InitializeComponent();

			checkBox_AlwaysTop.Checked = IDE.Global.IDEConfiguration.InfoBox_AlwaysOnTop;
			checkBox_CloseTabs.Checked = IDE.Global.IDEConfiguration.InfoBox_CloseTabsOnClose;
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
			if (IDE.Global.IDEConfiguration.InfoBox_CloseTabsOnClose)
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
			IDE.Global.IDEConfiguration.InfoBox_AlwaysOnTop = checkBox_AlwaysTop.Checked;

			TopMost = IDE.Global.IDEConfiguration.InfoBox_AlwaysOnTop;
		}

		private void checkBox_CloseTabs_CheckedChanged(object sender, EventArgs e) =>
			IDE.Global.IDEConfiguration.InfoBox_CloseTabsOnClose = checkBox_CloseTabs.Checked;

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

			var newTabPage = new TabPage(flag.ToUpper())
			{
				UseVisualStyleBackColor = false,
				BackColor = Color.FromArgb(42, 42, 42),
				Size = tabControl.Size,
				Padding = new Padding(5)
			};

			var textBox = new RichTextBox
			{
				Text = RddaReader.GetKeywordDescription(flag.TrimEnd('='), type),
				ForeColor = Color.Gainsboro,
				BackColor = Color.FromArgb(48, 48, 48),
				Font = new Font("Segoe UI", 12f),
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
