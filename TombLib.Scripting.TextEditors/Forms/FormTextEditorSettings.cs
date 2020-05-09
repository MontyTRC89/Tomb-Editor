using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombLib.Scripting.TextEditors.Controls.Settings;

namespace TombLib.Scripting.TextEditors.Forms
{
	// TODO: Refactor

	public partial class FormTextEditorSettings : DarkForm
	{
		private SettingsClassicScript settings_ClassicScript;

		private TextEditorConfigs _configs;
		private TextEditorConfigs _configsCopy;

		public FormTextEditorSettings()
		{
			InitializeComponent();

			_configs = TextEditorConfigs.Load();
			_configsCopy = TextEditorConfigs.Load();

			settings_ClassicScript = new SettingsClassicScript(_configs.ClassicScript);
			tabPage_ClassicScript.Controls.Add(settings_ClassicScript);

			DarkTreeNode classicScriptNode = new DarkTreeNode("Classic Script");
			DarkTreeNode luaNode = new DarkTreeNode("Lua");

			treeView.Nodes.Add(classicScriptNode);
			treeView.Nodes.Add(luaNode);

			treeView.SelectNode(classicScriptNode);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			settings_ClassicScript.ForcePreviewUpdate();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (DialogResult == DialogResult.OK)
			{
				settings_ClassicScript.ApplySettings();
			}
			else
			{
				_configs = _configsCopy;

				_configs.ClassicScript.Save();
				_configs.Lua.Save();
			}
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			_configs.ClassicScript.Save();
		}

		private void button_ResetDefault_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this,
				"Are you sure you want to reset all settings to default?", "Reset?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				settings_ClassicScript.ResetToDefault();
			}
		}

		private void treeView_SelectedNodesChanged(object sender, EventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (treeView.SelectedNodes[0] == treeView.Nodes[0])
			{
				tablessTabControl.SelectTab(1);
				settings_ClassicScript.ForcePreviewUpdate();
			}
			else if (treeView.SelectedNodes[0] == treeView.Nodes[1])
				tablessTabControl.SelectTab(2);
		}
	}
}
