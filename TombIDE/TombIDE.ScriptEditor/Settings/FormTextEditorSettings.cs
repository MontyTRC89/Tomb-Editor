using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Windows.Forms;
using ClassicScriptConfig = TombLib.Scripting.ClassicScript.CS_EditorConfiguration;
using LuaConfig = TombLib.Scripting.Lua.Lua_EditorConfiguration;

namespace TombIDE.ScriptEditor.Settings
{
	// TODO: Refactor

	public partial class FormTextEditorSettings : DarkForm
	{
		private ClassicScriptConfig _classicScriptConfig = new ClassicScriptConfig();
		private LuaConfig _luaConfig = new LuaConfig();

		private readonly ClassicScriptConfig _classicScriptConfigCopy = new ClassicScriptConfig();
		private readonly LuaConfig _luaConfigCopy = new LuaConfig();

		public FormTextEditorSettings()
		{
			InitializeComponent();

			settingsClassicScript.Initialize(_classicScriptConfig);

			var classicScriptNode = new DarkTreeNode("Classic Script");
			var luaNode = new DarkTreeNode("Lua");

			treeView.Nodes.Add(classicScriptNode);
			treeView.Nodes.Add(luaNode);

			treeView.SelectNode(classicScriptNode);
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (DialogResult == DialogResult.OK)
			{
				settingsClassicScript.ApplySettings(_classicScriptConfig);
			}
			else
			{
				_classicScriptConfigCopy.Save();
				_luaConfigCopy.Save();
			}
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			_classicScriptConfig.Save();
			_luaConfig.Save();
		}

		private void button_ResetDefault_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this,
				"Are you sure you want to reset all settings to default?", "Reset?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result == DialogResult.Yes)
			{
				settingsClassicScript.ResetToDefault();
			}
		}

		private void treeView_SelectedNodesChanged(object sender, EventArgs e)
		{
			if (treeView.SelectedNodes.Count == 0)
				return;

			if (treeView.SelectedNodes[0] == treeView.Nodes[0])
			{
				tablessTabControl.SelectTab(1);
				settingsClassicScript.ForcePreviewUpdate();
			}
			else if (treeView.SelectedNodes[0] == treeView.Nodes[1])
				tablessTabControl.SelectTab(2);
		}
	}
}
